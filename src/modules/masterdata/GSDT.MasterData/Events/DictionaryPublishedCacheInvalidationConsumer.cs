using MassTransit;

namespace GSDT.MasterData.Events;

/// <summary>
/// MassTransit consumer: invalidates in-process cache for a dictionary code
/// when DictionaryPublishedEvent is received.
/// Cache key pattern: "masterdata:dict:{code}" — matches keys set by dictionary query handlers.
/// </summary>
public sealed class DictionaryPublishedCacheInvalidationConsumer(
    IMemoryCache cache,
    ILogger<DictionaryPublishedCacheInvalidationConsumer> logger)
    : IConsumer<DictionaryPublishedEvent>
{
    public Task Consume(ConsumeContext<DictionaryPublishedEvent> context)
    {
        var evt = context.Message;
        var cacheKey = $"masterdata:dict:{evt.DictionaryCode}";
        cache.Remove(cacheKey);

        logger.LogInformation(
            "Dictionary cache invalidated. Code={Code} TenantId={TenantId} Version={Version}",
            evt.DictionaryCode, evt.TenantId, evt.VersionNumber);

        return Task.CompletedTask;
    }
}
