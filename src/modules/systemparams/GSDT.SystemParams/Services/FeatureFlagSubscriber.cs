using StackExchange.Redis;

namespace GSDT.SystemParams.Services;

/// <summary>
/// BackgroundService — subscribes to Redis "ff-invalidate" channel.
/// On message: removes flagKey from FeatureFlagService L0 dict (&lt;100ms cross-pod propagation).
/// Gracefully degrades if Redis unavailable (5min reload is the fallback guarantee).
/// </summary>
public class FeatureFlagSubscriber(
    IConnectionMultiplexer redis,
    IFeatureFlagService featureFlagService,
    ILogger<FeatureFlagSubscriber> logger) : BackgroundService
{
    private const string FfInvalidateChannel = "ff-invalidate";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var sub = redis.GetSubscriber();
            await sub.SubscribeAsync(
                RedisChannel.Literal(FfInvalidateChannel),
                (_, message) =>
                {
                    var key = (string?)message;
                    if (key is not null)
                    {
                        featureFlagService.Invalidate(key);
                        logger.LogDebug("ff-invalidate received for key: {Key}", key);
                    }
                });

            logger.LogInformation("FeatureFlagSubscriber listening on Redis channel '{Channel}'", FfInvalidateChannel);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { /* shutdown */ }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "FeatureFlagSubscriber failed to subscribe — 5min reload remains active");
        }
    }
}
