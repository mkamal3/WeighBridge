using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DeltaToSqlitePoc.Configuration;
using DeltaToSqlitePoc.Delta;
using DeltaToSqlitePoc.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace DeltaToSqlitePoc.Services;

/// <summary>
/// Reads Synapse Link-style Delta Lake tables from ADLS Gen2 via Azure.Storage.Blobs.
///
/// Snapshot resolution (<see cref="ReadSnapshotAsync"/>) is checkpoint-aware: it seeds the
/// active file set from the latest <c>_delta_log</c> checkpoint (via <see cref="DeltaCheckpointReader"/>)
/// and then replays only the trailing JSON commits after that checkpoint's version
/// (via <see cref="DeltaLogParser"/>). This matters because Synapse Link prunes older commit
/// JSON files once a checkpoint exists — reading JSON commits alone (the original bug) misses
/// everything the checkpoint already consolidated, which is why full sync saw "0 active files".
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

    /// <summary>
    /// Resolves the current Delta table snapshot: seeds active files from the latest checkpoint
    /// (if any), then replays trailing <c>_delta_log</c> JSON commits after that version.
    /// </summary>
    public async Task<DeltaTableSnapshot> ReadSnapshotAsync(string deltaTablePath, CancellationToken ct)
    {
        var root = NormalizePath(deltaTablePath);
        var logPrefix = $"{root}/_delta_log/";

        _logger.LogInformation("Reading Delta log at {LogPrefix}", logPrefix);

        var logBlobNames = new List<string>();
        try
        {
            await foreach (var blob in ListBlobsAsync(logPrefix, ct).ConfigureAwait(false))
            {
                logBlobNames.Add(blob.Name);
            }
        }
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            throw CreateAuthException(ex);
        }

        if (logBlobNames.Count == 0)
        {
            throw new InvalidOperationException($"No files found at '{logPrefix}'. Is this a Delta table?");
        }

        var checkpointVersion = await TryGetLatestCheckpointVersionAsync(logBlobNames, ct).ConfigureAwait(false);

        var seedActive = new Dictionary<string, DeltaDataFile>(StringComparer.OrdinalIgnoreCase);
        string? seedSchema = null;
        long seedVersion = -1;

        if (checkpointVersion is long cv)
        {
            var checkpointFiles = logBlobNames
                .Where(n => DeltaCheckpointReader.TryParseCheckpointFileName(Path.GetFileName(n), out var v, out _, out _) && v == cv)
                .ToList();

            if (checkpointFiles.Count == 0)
            {
                _logger.LogWarning(
                    "'_last_checkpoint' points at version {Version} but no matching checkpoint parquet file(s) were found; falling back to full JSON replay.",
                    cv);
            }
            else
            {
                _logger.LogInformation(
                    "Found Delta checkpoint at version {Version} ({PartCount} part(s)); using it as the base file state.",
                    cv,
                    checkpointFiles.Count);

                foreach (var name in checkpointFiles)
                {
                    await using var stream = await OpenBlobStreamByNameAsync(name, ct).ConfigureAwait(false);
                    var (checkpointActive, schemaString) = await DeltaCheckpointReader.ReadAsync(stream, ct).ConfigureAwait(false);
                    foreach (var (path, file) in checkpointActive)
                    {
                        seedActive[path] = file;
                    }

                    seedSchema ??= schemaString;
                }

                seedVersion = cv;
            }
        }

        var trailingCommits = new List<(string FileName, string JsonContent)>();
        foreach (var name in logBlobNames)
        {
            var fileName = Path.GetFileName(name);
            if (!DeltaLogParser.TryParseVersion(fileName, out var version) || version <= seedVersion)
            {
                continue;
            }

            var content = await DownloadTextAsync(name, ct).ConfigureAwait(false);
            trailingCommits.Add((fileName, content));
        }

        _logger.LogInformation(
            "Replaying {CommitCount} trailing JSON commit(s) after checkpoint version {CheckpointVersion}",
            trailingCommits.Count,
            checkpointVersion?.ToString() ?? "none");

        var snapshot = DeltaLogParser.BuildSnapshot(root, trailingCommits, seedActive, seedVersion, seedSchema);

        _logger.LogInformation(
            "Delta snapshot resolved: version={Version}, activeFiles={FileCount}",
            snapshot.Version,
            snapshot.DataFiles.Count);

        return snapshot;
    }

    /// <summary>
    /// Reads <c>_last_checkpoint</c> for the checkpoint version if present, otherwise falls back
    /// to scanning <c>_delta_log</c> file names for the highest <c>*.checkpoint*.parquet</c> version.
    /// </summary>
    private async Task<long?> TryGetLatestCheckpointVersionAsync(List<string> logBlobNames, CancellationToken ct)
    {
        var lastCheckpointBlob = logBlobNames.FirstOrDefault(
            n => Path.GetFileName(n).Equals("_last_checkpoint", StringComparison.OrdinalIgnoreCase));

        if (lastCheckpointBlob is not null)
        {
            try
            {
                var json = await DownloadTextAsync(lastCheckpointBlob, ct).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("version", out var v) && v.TryGetInt64(out var version))
                {
                    return version;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse '_last_checkpoint'; falling back to scanning for checkpoint files.");
            }
        }

        long? maxVersion = null;
        foreach (var name in logBlobNames)
        {
            if (DeltaCheckpointReader.TryParseCheckpointFileName(Path.GetFileName(name), out var version, out _, out _))
            {
                maxVersion = maxVersion is null ? version : Math.Max(maxVersion.Value, version);
            }
        }

        return maxVersion;
    }

    private async IAsyncEnumerable<BlobItem> ListBlobsAsync(
        string prefix,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var page in _container
            .GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, ct)
            .AsPages()
            .WithCancellation(ct))
        {
            foreach (var blob in page.Values)
            {
                yield return blob;
            }
        }
    }

    private async Task<string> DownloadTextAsync(string blobName, CancellationToken ct)
    {
        await using var stream = await OpenBlobStreamByNameAsync(blobName, ct).ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(ct).ConfigureAwait(false);
    }

    private async Task<Stream> OpenBlobStreamByNameAsync(string blobName, CancellationToken ct)
    {
        var client = _container.GetBlobClient(blobName);
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

    public Task<Stream> OpenParquetStreamAsync(string tableRoot, string relativePath, CancellationToken ct)
    {
        var blobPath = Combine(NormalizePath(tableRoot), relativePath.Replace('\\', '/'));
        _logger.LogDebug("Downloading parquet blob {BlobPath}", blobPath);
        return OpenBlobStreamByNameAsync(blobPath, ct);
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
