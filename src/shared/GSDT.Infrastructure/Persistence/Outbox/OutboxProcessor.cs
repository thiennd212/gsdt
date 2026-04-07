
namespace GSDT.Infrastructure.Persistence.Outbox;

/// <summary>
/// Stub background service that polls for unprocessed OutboxMessages.
/// Phase 02c replaces dispatch logic with MassTransit EF Outbox (production-grade at-least-once).
/// This stub exists so the DI graph compiles and tests can run without MassTransit.
/// </summary>
public sealed class OutboxProcessor(
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "OutboxProcessor started (stub). Production dispatch handled by MassTransit EF Outbox (Phase 02c). " +
            "Polling interval: {IntervalSeconds}s", options.Value.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(options.Value.PollingIntervalSeconds),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown — expected on stoppingToken cancellation
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxProcessor encountered an unexpected error.");
            }
        }

        logger.LogInformation("OutboxProcessor stopped.");
    }
}
