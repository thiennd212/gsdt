namespace GSDT.MasterData.Controllers;

/// <summary>
/// Read-only endpoint for tenant-scoped dynamic catalogs (managed per-tenant).
/// Route: GET api/v1/masterdata/catalogs/{type}
/// Returns CatalogItemDto[] filtered by current tenant.
/// Auth: any authenticated user.
/// </summary>
[ApiController]
[Route("api/v1/masterdata/catalogs")]
[Authorize]
public sealed class DynamicCatalogsController(
    IReadDbConnection db,
    ICurrentUser currentUser) : ApiControllerBase
{
    /// <summary>Map FE slug → DB table name in masterdata schema.</summary>
    private static readonly Dictionary<string, string> SlugToTable = new(StringComparer.OrdinalIgnoreCase)
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
        if (!SlugToTable.TryGetValue(type, out var tableName))
            return NotFound(new { error = $"Unknown catalog type: '{type}'." });

        var tenantId = currentUser.TenantId;

        // Tenant-scoped catalogs: filter by TenantId. If no tenant, return empty.
        var sql = tenantId.HasValue
            ? $"SELECT Id, Code, Name, IsActive FROM [masterdata].[{tableName}] WHERE IsDeleted = 0 AND TenantId = @TenantId ORDER BY Name"
            : $"SELECT Id, Code, Name, IsActive FROM [masterdata].[{tableName}] WHERE IsDeleted = 0 ORDER BY Name";

        var items = await db.QueryAsync<CatalogItemDto>(
            sql,
            tenantId.HasValue ? new { TenantId = tenantId.Value } : null,
            ct);

        return Ok(ApiResponse<IEnumerable<CatalogItemDto>>.Ok(items));
    }

    private sealed record CatalogItemDto(Guid Id, string Code, string Name, bool IsActive);
}
