using System.Text.Json;
using StackExchange.Redis;

namespace GSDT.Identity.Infrastructure.Services;

/// <summary>
/// Resolves a user's effective permissions by merging:
///   1. Direct roles (AspNetUserRoles)
///   2. Group roles (UserGroupMembership → GroupRoleAssignment)
///   3. Active delegations (UserDelegation where ValidFrom≤Now≤ValidTo, !IsRevoked)
///
/// All role IDs are unioned → permission codes loaded from RolePermission join Permission.
/// Cache: Redis key "perm:{userId}", TTL 10 min.
/// Call InvalidateAsync after any role/group/delegation change.
/// </summary>
public sealed class EffectivePermissionService : IEffectivePermissionService
{
    private const int CacheTtlSeconds = 600; // 10 min
    private const string CacheKeyPrefix = "perm:";

    private readonly IdentityDbContext _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<EffectivePermissionService> _logger;

    public EffectivePermissionService(
        IdentityDbContext db,
        IConnectionMultiplexer redis,
        ILogger<EffectivePermissionService> logger)
    {
        _db = db;
        _redis = redis;
        _logger = logger;
    }

    public async Task<EffectivePermissionSummary> GetSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var cached = await TryGetCachedAsync(userId);
        if (cached is not null) return cached;

        var summary = await BuildFromDbAsync(userId, ct);
        await TryCacheAsync(userId, summary);
        return summary;
    }

    public async Task InvalidateAsync(Guid userId)
    {
        try
        {
            var redisDb = _redis.GetDatabase();
            await redisDb.KeyDeleteAsync($"{CacheKeyPrefix}{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable invalidating perm:{UserId}", userId);
        }
    }

    // --- DB resolution ---

    private async Task<EffectivePermissionSummary> BuildFromDbAsync(Guid userId, CancellationToken ct)
    {
        // 1. Direct roles — C1 fix: filter IsActive to exclude soft-deleted roles
        var directRoles = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles.Where(r => r.IsActive),
                  ur => ur.RoleId,
                  r => r.Id,
                  (_, r) => new RoleInfo(r.Id, r.Code, r.Name ?? string.Empty, r.RoleType))
            .ToListAsync(ct);

        // 2. Group roles (membership → assignment → role) — C1 fix: filter IsActive
        var groupRoles = await _db.UserGroupMemberships
            .Where(m => m.UserId == userId)
            .Join(_db.GroupRoleAssignments,
                  m => m.GroupId,
                  gra => gra.GroupId,
                  (m, gra) => new { m.GroupId, m.Group, gra.RoleId, gra.Role })
            .Where(x => x.Role.IsActive)
            .Select(x => new GroupRoleInfo(
                x.GroupId,
                x.Group.Code,
                x.RoleId,
                x.Role.Code))
            .ToListAsync(ct);

        // 3. Active delegations where this user is the delegate
        var now = DateTime.UtcNow;
        var delegations = await _db.UserDelegations
            .Where(d => d.DelegateId == userId
                        && !d.IsRevoked
                        && d.ValidFrom <= now
                        && d.ValidTo >= now)
            .Select(d => new DelegationInfo(
                d.Id,
                d.DelegatorId,
                d.Delegator.FullName,
                d.ValidFrom,
                d.ValidTo))
            .ToListAsync(ct);

        // 4. Union all role IDs (direct + group + delegator roles)
        var directRoleIds = directRoles.Select(r => r.RoleId).ToHashSet();
        var groupRoleIds = groupRoles.Select(r => r.RoleId).ToHashSet();
        var delegatorIds = delegations.Select(d => d.DelegatorId).ToList();

        var delegatorRoleIds = delegatorIds.Count > 0
            ? await _db.UserRoles
                .Where(ur => delegatorIds.Contains(ur.UserId))
                .Select(ur => ur.RoleId)
                .Distinct()
                .ToListAsync(ct)
            : [];

        var allRoleIds = directRoleIds
            .Union(groupRoleIds)
            .Union(delegatorRoleIds)
            .ToList();

        // 5. Load permission codes from all role IDs
        var permissionCodes = allRoleIds.Count > 0
            ? await _db.RolePermissions
                .Where(rp => allRoleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync(ct)
            : [];

        return new EffectivePermissionSummary
        {
            UserId = userId,
            DirectRoles = directRoles,
            GroupRoles = groupRoles,
            ActiveDelegations = delegations,
            PermissionCodes = permissionCodes.ToHashSet()
        };
    }

    // --- Redis helpers ---

    private async Task<EffectivePermissionSummary?> TryGetCachedAsync(Guid userId)
    {
        try
        {
            var redisDb = _redis.GetDatabase();
            var cached = await redisDb.StringGetAsync($"{CacheKeyPrefix}{userId}");
            if (cached.HasValue)
                return JsonSerializer.Deserialize<EffectivePermissionSummary>(cached.ToString(), JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable reading perm:{UserId}, falling back to DB", userId);
        }
        return null;
    }

    private async Task TryCacheAsync(Guid userId, EffectivePermissionSummary summary)
    {
        try
        {
            var redisDb = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(summary, JsonOptions);
            await redisDb.StringSetAsync(
                $"{CacheKeyPrefix}{userId}",
                json,
                TimeSpan.FromSeconds(CacheTtlSeconds));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable writing perm:{UserId}", userId);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        // IReadOnlySet<string> cannot be deserialized directly — converters handle abstract types.
        // HashSet<string> is the concrete type used at serialization time (BuildFromDbAsync assigns .ToHashSet()).
        Converters = { new ReadOnlySetJsonConverter() },
    };
}

/// <summary>
/// Converts JSON arrays to/from IReadOnlySet{string}.
/// System.Text.Json cannot deserialize abstract collection interfaces directly;
/// this converter maps IReadOnlySet{string} ↔ HashSet{string} transparently.
/// </summary>
internal sealed class ReadOnlySetJsonConverter : System.Text.Json.Serialization.JsonConverter<IReadOnlySet<string>>
{
    public override IReadOnlySet<string> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var list = JsonSerializer.Deserialize<HashSet<string>>(ref reader, options);
        return list ?? new HashSet<string>();
    }

    public override void Write(
        Utf8JsonWriter writer,
        IReadOnlySet<string> value,
        JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, typeof(HashSet<string>), options);
}
