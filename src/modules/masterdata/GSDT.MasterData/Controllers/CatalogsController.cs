namespace GSDT.MasterData.Controllers;

/// <summary>
/// CRUD endpoints for the 10 tenant-scoped dynamic catalogs.
/// Route: api/v1/masterdata/catalogs/{catalogType}
/// catalogType values: managing-authorities, national-target-programs, project-owners,
/// project-management-units, investment-decision-authorities, contractors,
/// document-types, project-implementation-statuses, banks, managing-agencies
/// Read: BTC + CQCQ + CDT. Write (POST/PUT/DELETE): BTC only.
/// </summary>
[ApiController]
[Route("api/v1/masterdata/catalogs")]
public class CatalogsController(MasterDataDbContext db) : ApiControllerBase
{
    // ── Catalog type resolution ───────────────────────────────────────────────

    private static readonly HashSet<string> ValidTypes =
    [
        "managing-authorities", "national-target-programs", "project-owners",
        "project-management-units", "investment-decision-authorities", "contractors",
        "document-types", "project-implementation-statuses", "banks", "managing-agencies"
    ];

    /// <summary>Returns the correct DbSet for the given catalogType string. Null if unknown.</summary>
    private IQueryable<TenantCatalog>? ResolveSet(string catalogType) => catalogType switch
    {
        "managing-authorities"            => db.ManagingAuthorities,
        "national-target-programs"        => db.NationalTargetPrograms,
        "project-owners"                  => db.ProjectOwners,
        "project-management-units"        => db.ProjectManagementUnits,
        "investment-decision-authorities" => db.InvestmentDecisionAuthorities,
        "contractors"                     => db.Contractors,
        "document-types"                  => db.DocumentTypes,
        "project-implementation-statuses" => db.ProjectImplementationStatuses,
        "banks"                           => db.Banks,
        "managing-agencies"               => db.ManagingAgencies,
        _                                 => null
    };

    // ── GET list ─────────────────────────────────────────────────────────────

    /// <summary>List items for the given catalog type scoped to the calling tenant.</summary>
    [HttpGet("{catalogType}")]
    [Authorize(Roles = "BTC,CQCQ,CDT")]
    public async Task<IActionResult> GetList(
        string catalogType,
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var set = ResolveSet(catalogType);
        if (set is null)
            return BadRequest(new { error = $"Unknown catalog type '{catalogType}'." });

        var tenantId = ResolveTenantId();

        var query = set.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var result = await query
            .OrderBy(x => x.Code)
            .Select(x => new { x.Id, x.Code, x.Name, x.IsActive })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(result));
    }

    // ── GET by ID ─────────────────────────────────────────────────────────────

    [HttpGet("{catalogType}/{id:guid}")]
    [Authorize(Roles = "BTC,CQCQ,CDT")]
    public async Task<IActionResult> GetById(
        string catalogType, Guid id, CancellationToken ct)
    {
        var set = ResolveSet(catalogType);
        if (set is null)
            return BadRequest(new { error = $"Unknown catalog type '{catalogType}'." });

        var tenantId = ResolveTenantId();
        var item = await set.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == id)
            .Select(x => new { x.Id, x.Code, x.Name, x.IsActive })
            .FirstOrDefaultAsync(ct);

        return item is null
            ? NotFound(new { error = $"Item '{id}' not found in '{catalogType}'." })
            : Ok(ApiResponse<object>.Ok(item));
    }

    // ── POST create ───────────────────────────────────────────────────────────

    [HttpPost("{catalogType}")]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> Create(
        string catalogType,
        [FromBody] UpsertCatalogRequest req,
        CancellationToken ct)
    {
        if (!ValidTypes.Contains(catalogType))
            return BadRequest(new { error = $"Unknown catalog type '{catalogType}'." });

        var tenantId = ResolveTenantId();

        // Guard duplicate code within tenant
        var set = ResolveSet(catalogType)!;
        if (await set.AnyAsync(x => x.TenantId == tenantId && x.Code == req.Code, ct))
            return Conflict(new { error = $"Code '{req.Code}' already exists in '{catalogType}'." });

        var entity = CreateEntity(catalogType, tenantId, req.Code, req.Name);
        if (entity is null)
            return BadRequest(new { error = $"Unknown catalog type '{catalogType}'." });

        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { entity.Id }));
    }

    // ── PUT update ────────────────────────────────────────────────────────────

    [HttpPut("{catalogType}/{id:guid}")]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> Update(
        string catalogType, Guid id,
        [FromBody] UpdateCatalogRequest req,
        CancellationToken ct)
    {
        var set = ResolveSet(catalogType);
        if (set is null)
            return BadRequest(new { error = $"Unknown catalog type '{catalogType}'." });

        var tenantId = ResolveTenantId();
        var item = await set.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (item is null)
            return NotFound(new { error = $"Item '{id}' not found in '{catalogType}'." });

        // Guard duplicate code (exclude self)
        if (item.Code != req.Code &&
            await set.AnyAsync(x => x.TenantId == tenantId && x.Code == req.Code && x.Id != id, ct))
            return Conflict(new { error = $"Code '{req.Code}' already exists in '{catalogType}'." });

        item.Code     = req.Code;
        item.Name     = req.Name;
        item.IsActive = req.IsActive;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── DELETE soft delete ────────────────────────────────────────────────────

    [HttpDelete("{catalogType}/{id:guid}")]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> SoftDelete(
        string catalogType, Guid id, CancellationToken ct)
    {
        var set = ResolveSet(catalogType);
        if (set is null)
            return BadRequest(new { error = $"Unknown catalog type '{catalogType}'." });

        var tenantId = ResolveTenantId();
        var item = await set.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (item is null)
            return NotFound(new { error = $"Item '{id}' not found in '{catalogType}'." });

        item.IsActive = false;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the correct concrete TenantCatalog subclass using the generic factory on the base.
    /// TenantCatalog.Create{T} calls InitNew which uses the protected Id setter inherited from Entity.
    /// </summary>
    private static TenantCatalog? CreateEntity(
        string catalogType, Guid tenantId, string code, string name) =>
        catalogType switch
        {
            "managing-authorities"            => TenantCatalog.Create<ManagingAuthority>(tenantId, code, name),
            "national-target-programs"        => TenantCatalog.Create<NationalTargetProgram>(tenantId, code, name),
            "project-owners"                  => TenantCatalog.Create<ProjectOwner>(tenantId, code, name),
            "project-management-units"        => TenantCatalog.Create<ProjectManagementUnit>(tenantId, code, name),
            "investment-decision-authorities" => TenantCatalog.Create<InvestmentDecisionAuthority>(tenantId, code, name),
            "contractors"                     => TenantCatalog.Create<Contractor>(tenantId, code, name),
            "document-types"                  => TenantCatalog.Create<DocumentType>(tenantId, code, name),
            "project-implementation-statuses" => TenantCatalog.Create<ProjectImplementationStatus>(tenantId, code, name),
            "banks"                           => TenantCatalog.Create<Bank>(tenantId, code, name),
            "managing-agencies"               => TenantCatalog.Create<ManagingAgency>(tenantId, code, name),
            _                                 => null
        };
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public sealed record UpsertCatalogRequest(string Code, string Name);
public sealed record UpdateCatalogRequest(string Code, string Name, bool IsActive = true);
