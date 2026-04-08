namespace GSDT.MasterData.Controllers;

/// <summary>
/// Read-only endpoint for all 14 system-wide seed catalogs.
/// Returns active items ordered by SortOrder.
/// Route: GET api/v1/masterdata/seed-catalogs/{catalogType}
/// All authenticated users (BTC, CQCQ, CDT) may read seed catalogs.
/// </summary>
[ApiController]
[Route("api/v1/masterdata/seed-catalogs")]
[Authorize(Roles = "BTC,CQCQ,CDT")]
public class SeedCatalogsController(MasterDataDbContext db) : ApiControllerBase
{
    private static readonly HashSet<string> ValidTypes =
    [
        "industry-sectors", "project-groups", "domestic-project-statuses",
        "oda-project-statuses", "adjustment-contents", "bid-selection-forms",
        "bid-selection-methods", "contract-forms", "bid-sector-types",
        "evaluation-types", "audit-conclusion-types", "violation-types",
        "violation-actions", "oda-project-types"
    ];

    /// <summary>
    /// Returns all active items for the given catalog type, ordered by SortOrder.
    /// catalogType values: industry-sectors, project-groups, domestic-project-statuses,
    /// oda-project-statuses, adjustment-contents, bid-selection-forms,
    /// bid-selection-methods, contract-forms, bid-sector-types, evaluation-types,
    /// audit-conclusion-types, violation-types, violation-actions, oda-project-types
    /// </summary>
    [HttpGet("{catalogType}")]
    public async Task<IActionResult> GetCatalog(string catalogType, CancellationToken ct)
    {
        if (!ValidTypes.Contains(catalogType))
            return BadRequest(new { error = $"Unknown catalog type '{catalogType}'." });

        var result = await QueryCatalog(catalogType, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    private Task<List<object>> QueryCatalog(string catalogType, CancellationToken ct) =>
        catalogType switch
        {
            "industry-sectors"           => Project(db.IndustrySectors, ct),
            "project-groups"             => Project(db.ProjectGroups, ct),
            "domestic-project-statuses"  => Project(db.DomesticProjectStatuses, ct),
            "oda-project-statuses"       => Project(db.OdaProjectStatuses, ct),
            "adjustment-contents"        => Project(db.AdjustmentContents, ct),
            "bid-selection-forms"        => Project(db.BidSelectionForms, ct),
            "bid-selection-methods"      => Project(db.BidSelectionMethods, ct),
            "contract-forms"             => Project(db.ContractForms, ct),
            "bid-sector-types"           => Project(db.BidSectorTypes, ct),
            "evaluation-types"           => Project(db.EvaluationTypes, ct),
            "audit-conclusion-types"     => Project(db.AuditConclusionTypes, ct),
            "violation-types"            => Project(db.ViolationTypes, ct),
            "violation-actions"          => Project(db.ViolationActions, ct),
            "oda-project-types"          => Project(db.OdaProjectTypes, ct),
            _                            => Task.FromResult(new List<object>())
        };

    /// <summary>Projects any seed-catalog DbSet to a lightweight DTO list.</summary>
    private static async Task<List<object>> Project<T>(
        IQueryable<T> set, CancellationToken ct)
        where T : Entity<Guid>
    {
        // All seed catalogs share Code/Name/IsActive/SortOrder — project via EF anonymous select.
        return await set.AsNoTracking()
            .Where(x => EF.Property<bool>(x, "IsActive"))
            .OrderBy(x => EF.Property<int>(x, "SortOrder"))
            .Select(x => (object)new
            {
                Id       = x.Id,
                Code     = EF.Property<string>(x, "Code"),
                Name     = EF.Property<string>(x, "Name"),
                SortOrder = EF.Property<int>(x, "SortOrder")
            })
            .ToListAsync(ct);
    }
}
