
namespace GSDT.SystemParams.Services;

/// <summary>
/// BackgroundService — full reload of feature flag L0 dict every 5min.
/// Guarantees eventual consistency even if Redis pub/sub message was missed.
/// </summary>
public class FeatureFlagRefreshService(
    IFeatureFlagService featureFlagService,
    ILogger<FeatureFlagRefreshService> logger) : BackgroundService
{
    private static readonly TimeSpan ReloadInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial load at startup
        await featureFlagService.ReloadAllAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(ReloadInterval, stoppingToken);
            try
            {
                await featureFlagService.ReloadAllAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "FeatureFlag reload failed — will retry in {Interval}", ReloadInterval);
            }
        }
    }
}
