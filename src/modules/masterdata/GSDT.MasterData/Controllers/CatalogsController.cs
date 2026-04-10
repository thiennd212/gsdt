namespace GSDT.MasterData.Controllers;

/// <summary>
/// Unified read-only endpoint for ALL masterdata catalogs.
/// Route: GET api/v1/masterdata/catalogs/{type}
///
/// Two catalog kinds served from one route:
///   - Seed catalogs (14): system-wide static reference data, no TenantId filter, sorted by SortOrder.
///   - Tenant catalogs (10): per-tenant managed data, filtered by current user's TenantId, sorted by Name.
///
/// FE calls useSeedCatalog/useDynamicCatalog → both hit this single endpoint.
/// Auth: any authenticated user (catalogs are read-only reference data).
/// </summary>
[ApiController]
[Route("api/v1/masterdata/catalogs")]
[Authorize]
public sealed class CatalogsController(
    IReadDbConnection db,
    ICurrentUser currentUser) : ApiControllerBase
{
    // ── Seed catalogs: system-wide, no TenantId, sorted by SortOrder ────────
    private static readonly Dictionary<string, string> SeedCatalogs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["project-groups"]            = "ProjectGroups",
        ["domestic-project-statuses"] = "DomesticProjectStatuses",
        ["industry-sectors"]          = "IndustrySectors",
        ["adjustment-contents"]       = "AdjustmentContents",
        ["bid-selection-forms"]       = "BidSelectionForms",
        ["bid-selection-methods"]     = "BidSelectionMethods",
        ["contract-forms"]            = "ContractForms",
        ["bid-sector-types"]          = "BidSectorTypes",
        ["oda-project-types"]         = "OdaProjectTypes",
        ["oda-project-statuses"]      = "OdaProjectStatuses",
        ["evaluation-types"]          = "EvaluationTypes",
        ["audit-conclusion-types"]    = "AuditConclusionTypes",
        ["violation-types"]           = "ViolationTypes",
        ["violation-actions"]         = "ViolationActions",
    };

    // ── Tenant catalogs: per-tenant, filtered by TenantId, sorted by Name ───
    private static readonly Dictionary<string, string> TenantCatalogs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["managing-authorities"]            = "ManagingAuthorities",
        ["national-target-programs"]        = "NationalTargetPrograms",
        ["project-owners"]                  = "ProjectOwners",
        ["project-management-units"]        = "ProjectManagementUnits",
        ["investment-decision-authorities"] = "InvestmentDecisionAuthorities",
        ["contractors"]                     = "Contractors",
        ["document-types"]                  = "DocumentTypes",
        ["project-implementation-statuses"] = "ProjectImplementationStatuses",
        ["banks"]                           = "Banks",
        ["managing-agencies"]               = "ManagingAgencies",
    };

    [HttpGet("{type}")]
    public async Task<IActionResult> GetByType(string type, CancellationToken ct)
    {
        // Seed catalog — system-wide, no tenant filter
        if (SeedCatalogs.TryGetValue(type, out var seedTable))
        {
            var sql = $"SELECT Id, Code, Name, IsActive FROM [masterdata].[{seedTable}] WHERE IsDeleted = 0 ORDER BY SortOrder, Name";
            var items = await db.QueryAsync<CatalogItemDto>(sql, ct);
            return Ok(ApiResponse<IEnumerable<CatalogItemDto>>.Ok(items));
        }

        // Tenant catalog — filtered by current user's tenant
        if (TenantCatalogs.TryGetValue(type, out var tenantTable))
        {
            var tenantId = currentUser.TenantId;
            var sql = tenantId.HasValue
                ? $"SELECT Id, Code, Name, IsActive FROM [masterdata].[{tenantTable}] WHERE IsDeleted = 0 AND TenantId = @TenantId ORDER BY Name"
                : $"SELECT Id, Code, Name, IsActive FROM [masterdata].[{tenantTable}] WHERE IsDeleted = 0 ORDER BY Name";
            var items = await db.QueryAsync<CatalogItemDto>(
                sql, tenantId.HasValue ? new { TenantId = tenantId.Value } : null, ct);
            return Ok(ApiResponse<IEnumerable<CatalogItemDto>>.Ok(items));
        }

        return NotFound(new { error = $"Unknown catalog type: '{type}'." });
    }

    private sealed record CatalogItemDto(Guid Id, string Code, string Name, bool IsActive);
}
