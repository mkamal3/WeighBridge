using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DeltaToSqlitePoc.Configuration;
using DeltaToSqlitePoc.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace DeltaToSqlitePoc.Services;

/// <summary>
/// Reads Synapse Link-style Delta Lake tables from ADLS Gen2 via Azure.Storage.Blobs.
/// Resolves active files from <c>_delta_log</c>, then streams Parquet with Parquet.Net.
/// </summary>
public sealed class AdlsDeltaTableReader
{
    private readonly SyncSettings _settings;
    private readonly ILogger<AdlsDeltaTableReader> _logger;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly BlobContainerClient _container;

    public AdlsDeltaTableReader(SyncSettings settings, ILogger<AdlsDeltaTableReader> logger)
    {
        _settings = settings;
        _logger = logger;
        _retryPipeline = AzureRetryPolicy.Create(settings.AzureRetryCount, logger);
        _container = CreateContainerClient(settings, logger);
    }

    private static BlobContainerClient CreateContainerClient(SyncSettings settings, ILogger logger)
    {
        if (!string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            logger.LogInformation("ADLS auth mode: connection string");
            var service = new BlobServiceClient(settings.ConnectionString);
            return service.GetBlobContainerClient(settings.ContainerName);
        }

        logger.LogInformation(
            "ADLS auth mode: DefaultAzureCredential (install Azure CLI + run 'az login', or set Sync:ConnectionString)");

        // Prefer interactive / CLI credentials for local dev; skip MI noise on developer machines.
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeManagedIdentityCredential = true,
            ExcludeWorkloadIdentityCredential = true,
            ExcludeInteractiveBrowserCredential = false
        });

        var serviceUri = string.IsNullOrWhiteSpace(settings.BlobServiceUri)
            ? new Uri($"https://{settings.StorageAccountName}.blob.core.windows.net")
            : new Uri(settings.BlobServiceUri);

        var blobService = new BlobServiceClient(serviceUri, credential);
        return blobService.GetBlobContainerClient(settings.ContainerName);
    }

    public async Task EnsureContainerAccessibleAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "Connecting to ADLS Gen2: account={Account}, container={Container}",
            _settings.StorageAccountName,
            _settings.ContainerName);

        try
        {
            await _retryPipeline.ExecuteAsync(
                async token =>
                {
                    var exists = await _container.ExistsAsync(token).ConfigureAwait(false);
                    if (!exists.Value)
                    {
                        throw new InvalidOperationException(
                            $"Container '{_settings.ContainerName}' was not found (or the identity lacks access).");
                    }
                },
                ct).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            throw CreateAuthException(ex);
        }
        catch (AuthenticationFailedException ex)
        {
            throw new InvalidOperationException(
                """
                Azure authentication failed. Fix one of:
                  1) Install Azure CLI and run: az login
                  2) Set user secret Sync:ConnectionString to the storage account connection string
                  3) In Visual Studio: Tools → Options → Azure Service Authentication → add account
                Also ensure your identity has role 'Storage Blob Data Reader' on the storage account/container.
                """,
                ex);
        }
    }

    private InvalidOperationException CreateAuthException(RequestFailedException ex) =>
        new(
            $"""
            Storage returned {ex.Status} ({ex.ErrorCode}). You authenticated, but this identity cannot read blobs.
            Grant role 'Storage Blob Data Reader' on storage account '{_settings.StorageAccountName}' (or the container).
            Owner/Contributor alone is not enough for Azure AD blob access.
            Or set Sync:ConnectionString via user-secrets for a local PoC.
            Original: {ex.Message}
            """,
            ex);

    public async Task<DeltaTableSnapshot> ReadSnapshotAsync(string deltaTablePath, CancellationToken ct)
    {
        var root = NormalizePath(deltaTablePath);
        var logPrefix = $"{root}/_delta_log/";

        _logger.LogInformation("Reading Delta log at {LogPrefix}", logPrefix);

        var commits = new List<(string FileName, string JsonContent)>();

        try
        {
            await foreach (var blob in ListBlobsAsync(logPrefix, ct).ConfigureAwait(false))
            {
                var name = Path.GetFileName(blob.Name);
                if (!name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith(".crc", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var content = await DownloadTextAsync(blob.Name, ct).ConfigureAwait(false);
                commits.Add((name, content));
            }
        }
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            throw CreateAuthException(ex);
        }

        if (commits.Count == 0)
        {
            throw new InvalidOperationException(
                $"No Delta commit JSON files found at '{logPrefix}'. Expected Synapse Link / Delta Lake layout.");
        }

        var snapshot = Delta.DeltaLogParser.BuildSnapshot(root, commits);
        _logger.LogInformation(
            "Delta snapshot version={Version}, activeFiles={FileCount}",
            snapshot.Version,
            snapshot.DataFiles.Count);

        return snapshot;
    }

    public async Task<IReadOnlyList<DeltaDataFile>> ReadFilesAddedAfterAsync(
        string deltaTablePath,
        long fromVersionExclusive,
        CancellationToken ct)
    {
        var root = NormalizePath(deltaTablePath);
        var logPrefix = $"{root}/_delta_log/";
        var commits = new List<(string FileName, string JsonContent)>();

        await foreach (var blob in ListBlobsAsync(logPrefix, ct).ConfigureAwait(false))
        {
            var name = Path.GetFileName(blob.Name);
            if (!name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var content = await DownloadTextAsync(blob.Name, ct).ConfigureAwait(false);
            commits.Add((name, content));
        }

        return Delta.DeltaLogParser.GetFilesAddedAfter(commits, fromVersionExclusive);
    }

    public async Task<Stream> OpenParquetStreamAsync(string tableRoot, string relativePath, CancellationToken ct)
    {
        var blobPath = Combine(NormalizePath(tableRoot), relativePath.Replace('\\', '/'));
        _logger.LogDebug("Downloading parquet blob {BlobPath}", blobPath);

        var client = _container.GetBlobClient(blobPath);
        var ms = new MemoryStream();

        await _retryPipeline.ExecuteAsync(
            async token =>
            {
                ms.SetLength(0);
                await client.DownloadToAsync(ms, token).ConfigureAwait(false);
            },
            ct).ConfigureAwait(false);

        ms.Position = 0;
        return ms;
    }

    private async IAsyncEnumerable<BlobItem> ListBlobsAsync(
        string prefix,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var pager = _container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, ct);
        await foreach (var blob in pager.ConfigureAwait(false))
        {
            yield return blob;
        }
    }

    private async Task<string> DownloadTextAsync(string blobPath, CancellationToken ct)
    {
        var client = _container.GetBlobClient(blobPath);
        return await _retryPipeline.ExecuteAsync(
            async token =>
            {
                var response = await client.DownloadContentAsync(token).ConfigureAwait(false);
                return response.Value.Content.ToString();
            },
            ct).ConfigureAwait(false);
    }

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/').Trim('/');

    private static string Combine(string root, string relative) =>
        string.IsNullOrEmpty(root) ? relative.TrimStart('/') : $"{root}/{relative.TrimStart('/')}";
}

public static class AzureRetryPolicy
{
    public static ResiliencePipeline Create(int retryCount, ILogger logger)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = Math.Max(1, retryCount),
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<RequestFailedException>(ex => IsTransient(ex))
                    .Handle<TimeoutException>()
                    .Handle<IOException>(),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        args.Outcome.Exception,
                        "Transient Azure error; retry {Attempt} after {Delay}",
                        args.AttemptNumber,
                        args.RetryDelay);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    private static bool IsTransient(RequestFailedException ex) =>
        ex.Status is 408 or 429 or 500 or 502 or 503 or 504
        || ex.ErrorCode is "ServerBusy" or "OperationTimedOut" or "InternalError";
}
