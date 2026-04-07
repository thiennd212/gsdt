
namespace GSDT.MasterData;

/// <summary>
/// Provides O(1) successor resolution and region grouping for VN admin reform.
/// Maps are built at startup and stored in IMemoryCache NeverRemove (~2.2MB RAM).
/// </summary>
public interface IAdministrativeUnitService
{
    /// <summary>Resolve old code → current active code (O(1) dict lookup).</summary>
    string ResolveActive(string code);

    /// <summary>Get all units by level (1=Province, 2=District, 3=Ward), optionally filtered by parent.</summary>
    IReadOnlyList<AdministrativeUnit> GetByLevel(int level, string? parentCode = null);

    /// <summary>Get all historical + current codes for a province (for cross-reform data queries).</summary>
    IReadOnlySet<string> GetRegionCodes(string provinceCode);
}

/// <summary>
/// Singleton — uses IServiceScopeFactory for scoped DB access during BuildCacheAsync only.
/// All runtime lookups use IMemoryCache (no DB dependency at request time).
/// </summary>
public class AdministrativeUnitService(
    IMemoryCache memoryCache,
    IServiceScopeFactory scopeFactory,
    ILogger<AdministrativeUnitService> logger) : IAdministrativeUnitService
{
    private const string SuccessorMapKey = "masterdata:successor_map";
    private const string RegionGroupMapKey = "masterdata:region_group_map";
    private const string AllUnitsKey = "masterdata:all_admin_units";

    public string ResolveActive(string code)
    {
        var map = memoryCache.Get<Dictionary<string, string>>(SuccessorMapKey)
                  ?? new Dictionary<string, string>();
        return map.TryGetValue(code, out var active) ? active : code;
    }

    public IReadOnlyList<AdministrativeUnit> GetByLevel(int level, string? parentCode = null)
    {
        var all = memoryCache.Get<List<AdministrativeUnit>>(AllUnitsKey) ?? [];
        return all
            .Where(u => u.Level == level && (parentCode == null || u.ParentCode == parentCode))
            .ToList();
    }

    public IReadOnlySet<string> GetRegionCodes(string provinceCode)
    {
        var map = memoryCache.Get<Dictionary<string, HashSet<string>>>(RegionGroupMapKey)
                  ?? new Dictionary<string, HashSet<string>>();
        return map.TryGetValue(provinceCode, out var codes)
            ? codes
            : new HashSet<string> { provinceCode };
    }

    /// <summary>
    /// Load all admin units from DB, build successor + region maps, cache as NeverRemove.
    /// Called once at startup after seeding completes.
    /// </summary>
    public async Task BuildCacheAsync(CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MasterDataDbContext>();
        var units = await db.AdministrativeUnits.AsNoTracking().ToListAsync(ct);
        logger.LogInformation("Building AdministrativeUnit cache maps from {Count} units", units.Count);

        // Build O(1) successor map: iterate chain until active unit reached
        var successorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var unit in units.Where(u => !u.IsActive && u.SuccessorCode != null))
        {
            // Follow chain iteratively (avoid recursion for long merge chains)
            var current = unit.SuccessorCode!;
            var visited = new HashSet<string> { unit.Code };
            while (true)
            {
                var next = units.FirstOrDefault(u => u.Code == current && !u.IsActive);
                if (next?.SuccessorCode == null || visited.Contains(next.SuccessorCode))
                    break;
                visited.Add(current);
                current = next.SuccessorCode;
            }
            successorMap[unit.Code] = current;
        }

        // Build region group map: provinceCode → all historical + current codes
        var regionGroupMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var province in units.Where(u => u.Level == 1))
        {
            var group = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { province.Code };
            // Add all codes that resolve to this province (via successor chain)
            foreach (var (old, active) in successorMap)
            {
                if (active == province.Code) group.Add(old);
            }
            regionGroupMap[province.Code] = group;
        }

        // Size = 1 per entry required when IMemoryCache.SizeLimit is configured (Cache:L1:MaxEntries)
        var opts = new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove, Size = 1 };
        memoryCache.Set(AllUnitsKey, units, opts);
        memoryCache.Set(SuccessorMapKey, successorMap, opts);
        memoryCache.Set(RegionGroupMapKey, regionGroupMap, opts);

        logger.LogInformation("AdministrativeUnit cache built: {SuccessorCount} successors, {ProvinceCount} province groups",
            successorMap.Count, regionGroupMap.Count);
    }
}
