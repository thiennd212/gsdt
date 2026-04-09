namespace GSDT.MasterData.Controllers;

/// <summary>
/// Read-only endpoint for system-wide seed catalogs (static reference data).
/// Route: GET api/v1/masterdata/seed-catalogs/{type}
/// Returns SeedCatalogItem[] for the given catalog type slug.
/// Auth: any authenticated user (no role restriction — seed data is read-only public reference).
/// Uses Dapper via IReadDbConnection to avoid DbContext DI conflicts.
/// </summary>
[ApiController]
[Route("api/v1/masterdata/seed-catalogs")]
[Authorize]
public sealed class SeedCatalogsController(IReadDbConnection db) : ApiControllerBase
{
    /// <summary>Map FE slug → DB table name in masterdata schema.</summary>
    private static readonly Dictionary<string, string> SlugToTable = new(StringComparer.OrdinalIgnoreCase)
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

    [HttpGet("{type}")]
    public async Task<IActionResult> GetByType(string type, CancellationToken ct)
    {
        if (!SlugToTable.TryGetValue(type, out var tableName))
            return NotFound(new { error = $"Unknown seed catalog type: '{type}'." });

        // All seed catalog tables share same schema: Id, Code, Name, IsActive, SortOrder
        var sql = $"SELECT Id, Code, Name, IsActive FROM [masterdata].[{tableName}] WHERE IsDeleted = 0 ORDER BY SortOrder, Name";
        var items = await db.QueryAsync<SeedCatalogItemDto>(sql, ct);

        return Ok(ApiResponse<IEnumerable<SeedCatalogItemDto>>.Ok(items));
    }

    private sealed record SeedCatalogItemDto(Guid Id, string Code, string Name, bool IsActive);
}
