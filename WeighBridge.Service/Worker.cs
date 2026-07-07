using WeighBridge.D365.Health;

namespace WeighBridge.Service;

public sealed class Worker(
    ILogger<Worker> logger,
    ID365ConnectionVerifier connectionVerifier) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await VerifyD365AuthenticationAsync(stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Queue polling and D365 submission will be implemented once SQLite is in place.
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task VerifyD365AuthenticationAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Verifying D365 authentication configuration...");

        var result = await connectionVerifier.VerifyAsync(cancellationToken).ConfigureAwait(false);

        if (result.Succeeded)
        {
            logger.LogInformation("D365 authentication is configured correctly.");
            return;
        }

        logger.LogWarning(
            "D365 authentication verification failed: {Error}. " +
            "The sync service will keep running; weighment sync will retry once connectivity and credentials are valid.",
            result.ErrorMessage);
    }
}
