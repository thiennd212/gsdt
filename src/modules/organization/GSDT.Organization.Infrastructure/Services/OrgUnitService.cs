using Dapper;

namespace GSDT.Organization.Infrastructure.Services;

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

    public async Task<IReadOnlyList<OrgUnitDto>> GetTreeAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cached = await cache.GetAsync<List<OrgUnitDto>>(TreeKey(tenantId), ct);
        if (cached is not null)
            return cached;

        // Load raw units with computed child/staff counts via subquery
        var rawUnits = await db.Query<OrgUnit>()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Level).ThenBy(x => x.Code)
            .Select(x => new
            {
                x.Id, x.ParentId, x.Name, x.NameEn,
                x.Code, x.Level, x.IsActive, x.TenantId, x.SuccessorId,
                ChildCount = db.OrgUnits.Count(c => c.ParentId == x.Id && !c.IsDeleted),
                StaffCount = db.Assignments.Count(a => a.OrgUnitId == x.Id && a.IsActive && !a.IsDeleted),
            })
            .ToListAsync(ct);

        var units = rawUnits.Select(x => new OrgUnitDto(
            x.Id, x.ParentId, x.Name, x.NameEn,
            x.Code, x.Level, x.IsActive, x.TenantId, x.SuccessorId,
            x.ChildCount, x.StaffCount)).ToList();

        await cache.SetAsync(TreeKey(tenantId), units, TreeTtl, ct);
        return units;
    }

    public async Task<IReadOnlyList<OrgUnitDto>> GetDescendantsAsync(
        Guid orgUnitId, Guid tenantId, CancellationToken ct = default)
    {
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

    public async Task<IReadOnlyList<Guid>> GetAncestorsAsync(
        Guid orgUnitId, Guid tenantId, CancellationToken ct = default)
    {
        var all = await GetTreeAsync(tenantId, ct);
        var map = all.ToDictionary(x => x.Id);
        var result = new List<Guid>();

        var current = orgUnitId;
        for (var depth = 0; depth < 20 && map.TryGetValue(current, out var node); depth++)
        {
            result.Add(node.Id);
            if (node.ParentId is null) break;
            current = node.ParentId.Value;
        }

        return result;
    }

    public async Task<OrgUnitDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await db.Query<OrgUnit>()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(x => new OrgUnitDto(
                x.Id, x.ParentId, x.Name, x.NameEn,
                x.Code, x.Level, x.IsActive, x.TenantId, x.SuccessorId))
            .FirstOrDefaultAsync(ct);

    public Task InvalidateCacheAsync(Guid tenantId, CancellationToken ct = default)
        => cache.RemoveAsync(TreeKey(tenantId), ct);

    public Task<bool> HasActiveChildrenAsync(Guid orgUnitId, CancellationToken ct = default)
        => db.Query<OrgUnit>().AnyAsync(x => x.ParentId == orgUnitId && x.IsActive, ct);

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
