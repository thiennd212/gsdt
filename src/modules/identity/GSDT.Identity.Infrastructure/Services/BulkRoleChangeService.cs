using StackExchange.Redis;

namespace GSDT.Identity.Infrastructure.Services;

/// <summary>
/// Invalidates Redis role cache for bulk user role changes.
/// Hangfire job — chunked 500 users/batch, Redis batch DEL user-roles:{userId} keys.
/// Effect: role cache expires within 5 minutes (ClaimsEnrichmentTransformer TTL).
/// </summary>
public sealed class BulkRoleChangeService
{
    private const int BatchSize = 500;

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<BulkRoleChangeService> _logger;

    public BulkRoleChangeService(IConnectionMultiplexer redis, ILogger<BulkRoleChangeService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Hangfire job entry point — scans all user-roles:* keys and deletes them.
    /// Use when a role's permissions change globally (e.g., role definition update).
    /// </summary>
    public async Task InvalidateRoleCacheForAllUsersAsync()
    {
        _logger.LogInformation("BulkRoleChangeService: invalidating all user-roles cache keys.");

        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var db = _redis.GetDatabase();

        var keys = server.Keys(pattern: "user-roles:*").ToList();
        var invalidated = 0;

        for (var i = 0; i < keys.Count; i += BatchSize)
        {
            var chunk = keys.Skip(i).Take(BatchSize).ToArray();
            await db.KeyDeleteAsync(chunk);
            invalidated += chunk.Length;
            _logger.LogDebug("BulkRoleChangeService: deleted {Count} keys (total {Total})", chunk.Length, invalidated);
        }

        _logger.LogInformation("BulkRoleChangeService: invalidated {Total} user-roles cache keys.", invalidated);
    }

    /// <summary>Invalidate role cache for a specific set of users — use after targeted bulk role assignment.</summary>
    public async Task InvalidateRoleCacheForUsersAsync(IReadOnlyList<Guid> userIds)
    {
        if (userIds.Count == 0) return;

        var db = _redis.GetDatabase();

        for (var i = 0; i < userIds.Count; i += BatchSize)
        {
            var chunk = userIds.Skip(i).Take(BatchSize)
                .Select(id => new RedisKey($"user-roles:{id}"))
                .ToArray();

            await db.KeyDeleteAsync(chunk);
        }

        _logger.LogInformation("BulkRoleChangeService: invalidated cache for {Count} users.", userIds.Count);
    }
}
