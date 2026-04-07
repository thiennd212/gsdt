
namespace GSDT.Organization;

/// <summary>
/// Core org unit service: cache-aside tree, recursive CTE descendants, ancestor walk.
/// Cache key pattern: "org:tree:{tenantId}" (TTL 15min).
/// IMPORTANT: Key includes tenantId — prevents cross-tenant cache leaks.
/// </summary>
public class OrgUnitService(
    OrgDbContext db,
    IReadDbConnection readDb,
    ICacheService cache)
{
    private static string TreeKey(Guid tenantId) => $"org:tree:{tenantId}";
    private static readonly TimeSpan TreeTtl = TimeSpan.FromMinutes(15);

    /// <summary>Returns full flat list of org units for a tenant (cache-aside, TTL 15min).</summary>
    public async Task<IReadOnlyList<OrgUnitDto>> GetTreeAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cached = await cache.GetAsync<List<OrgUnitDto>>(TreeKey(tenantId), ct);
        if (cached is not null)
            return cached;

        var units = await db.Query<OrgUnit>()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Level).ThenBy(x => x.Code)
            .Select(x => new OrgUnitDto(
                x.Id, x.ParentId, x.Name, x.NameEn,
                x.Code, x.Level, x.IsActive, x.TenantId, x.SuccessorId))
            .ToListAsync(ct);

        await cache.SetAsync(TreeKey(tenantId), units, TreeTtl, ct);
        return units;
    }

    /// <summary>Returns all descendants of a given org unit via recursive CTE (Dapper).</summary>
    public async Task<IReadOnlyList<OrgUnitDto>> GetDescendantsAsync(
        Guid orgUnitId, Guid tenantId, CancellationToken ct = default)
    {
        // SQL Server CTE recursion limit is 100 by default — sufficient for GOV depth ≤ 6
        const string sql = """
            WITH OrgHierarchy AS (
                SELECT Id, ParentId, Name, NameEn, Code, Level, IsActive, TenantId, SuccessorId
                FROM organization.OrgUnits
                WHERE Id = @RootId AND TenantId = @TenantId AND IsDeleted = 0
                UNION ALL
                SELECT o.Id, o.ParentId, o.Name, o.NameEn, o.Code, o.Level, o.IsActive, o.TenantId, o.SuccessorId
                FROM organization.OrgUnits o
                INNER JOIN OrgHierarchy h ON o.ParentId = h.Id
                WHERE o.TenantId = @TenantId AND o.IsDeleted = 0
            )
            SELECT * FROM OrgHierarchy;
            """;

        var rows = await readDb.QueryAsync<OrgUnitRow>(sql, new { RootId = orgUnitId, TenantId = tenantId }, ct);
        return rows.Select(r => new OrgUnitDto(r.Id, r.ParentId, r.Name, r.NameEn, r.Code, r.Level, r.IsActive, r.TenantId, r.SuccessorId))
                   .ToList();
    }

    /// <summary>Returns ancestor chain [current, parent, grandparent, ...] up to root.</summary>
    public async Task<IReadOnlyList<Guid>> GetAncestorsAsync(
        Guid orgUnitId, Guid tenantId, CancellationToken ct = default)
    {
        var all = await GetTreeAsync(tenantId, ct);
        var map = all.ToDictionary(x => x.Id);
        var result = new List<Guid>();

        var current = orgUnitId;
        // Guard: max depth 20 to prevent infinite loop on corrupted data
        for (var depth = 0; depth < 20 && map.TryGetValue(current, out var node); depth++)
        {
            result.Add(node.Id);
            if (node.ParentId is null) break;
            current = node.ParentId.Value;
        }

        return result;
    }

    /// <summary>Returns a single org unit by Id, null if not found.</summary>
    public async Task<OrgUnitDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await db.Query<OrgUnit>()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(x => new OrgUnitDto(
                x.Id, x.ParentId, x.Name, x.NameEn,
                x.Code, x.Level, x.IsActive, x.TenantId, x.SuccessorId))
            .FirstOrDefaultAsync(ct);

    /// <summary>Invalidates the cached tree for a tenant — call after any admin mutation.</summary>
    public Task InvalidateCacheAsync(Guid tenantId, CancellationToken ct = default)
        => cache.RemoveAsync(TreeKey(tenantId), ct);

    /// <summary>Returns true if the org unit has any active children.</summary>
    public Task<bool> HasActiveChildrenAsync(Guid orgUnitId, CancellationToken ct = default)
        => db.Query<OrgUnit>().AnyAsync(x => x.ParentId == orgUnitId && x.IsActive, ct);

    // Internal Dapper projection row — not exposed externally
    private sealed class OrgUnitRow
    {
        public Guid Id { get; init; }
        public Guid? ParentId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public int Level { get; init; }
        public bool IsActive { get; init; }
        public Guid TenantId { get; init; }
        public Guid? SuccessorId { get; init; }
    }
}
