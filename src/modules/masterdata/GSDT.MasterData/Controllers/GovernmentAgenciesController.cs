namespace GSDT.MasterData.Controllers;

/// <summary>
/// CRUD + tree endpoints for Cơ quan chủ quản (GovernmentAgency).
/// Route: api/v1/masterdata/government-agencies
/// Hierarchical catalog — supports flat list and in-memory tree (max 4 levels).
/// </summary>
[ApiController]
[Route("api/v1/masterdata/government-agencies")]
[Authorize(Roles = "BTC,CQCQ,CDT")]
public class GovernmentAgenciesController(MasterDataDbContext db) : ApiControllerBase
{
    // ── GET list ──────────────────────────────────────────────────────────────

    /// <summary>Flat list of all government agencies for the calling tenant.</summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var tenantId = ResolveTenantId();

        var query = db.GovernmentAgencies.AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var result = await query
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Id, x.Name, x.Code, x.ParentId, x.AgencyType,
                x.SortOrder, x.ReportDisplayOrder, x.IsActive
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(result));
    }

    // ── GET tree ──────────────────────────────────────────────────────────────

    /// <summary>Build in-memory tree from active agencies (roots → children, max 4 levels).</summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var all = await db.GovernmentAgencies.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.Name, x.Code, x.ParentId, x.AgencyType, x.SortOrder })
            .ToListAsync(ct);

        // Build lookup then recurse (typed to avoid dynamic performance overhead)
        var lookup = all.ToLookup(x => x.ParentId);

        List<object> BuildChildren(Guid? parentId, int depth)
        {
            if (depth > 4) return [];
            return lookup[parentId]
                .Select(item => (object)new
                {
                    item.Id, item.Name, item.Code, item.AgencyType, item.SortOrder,
                    Children = BuildChildren(item.Id, depth + 1)
                })
                .ToList();
        }

        var roots = BuildChildren(null, 1);
        return Ok(ApiResponse<object>.Ok(roots));
    }

    // ── GET by ID ─────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var item = await db.GovernmentAgencies.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == id)
            .Select(x => new
            {
                x.Id, x.Name, x.Code, x.ParentId, x.AgencyType,
                x.Origin, x.LdaServer, x.Address, x.Phone, x.Fax,
                x.Email, x.Notes, x.SortOrder, x.ReportDisplayOrder, x.IsActive
            })
            .FirstOrDefaultAsync(ct);

        return item is null
            ? NotFound(new { error = $"GovernmentAgency '{id}' not found." })
            : Ok(ApiResponse<object>.Ok(item));
    }

    // ── POST create ───────────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> Create(
        [FromBody] CreateGovernmentAgencyRequest req,
        CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        // Guard duplicate code within tenant
        if (await db.GovernmentAgencies.AnyAsync(
                x => x.TenantId == tenantId && x.Code == req.Code, ct))
            return Conflict(new { error = $"Code '{req.Code}' already exists." });

        // Validate parentId exists and belongs to same tenant (no self-ref cycles at create)
        if (req.ParentId.HasValue)
        {
            var parentExists = await db.GovernmentAgencies.AnyAsync(
                x => x.TenantId == tenantId && x.Id == req.ParentId.Value, ct);
            if (!parentExists)
                return BadRequest(new { error = $"ParentId '{req.ParentId}' does not exist." });
        }

        var agency = GovernmentAgency.Create(tenantId, req.Name, req.Code, req.ParentId);
        agency.AgencyType         = req.AgencyType;
        agency.Origin             = req.Origin;
        agency.LdaServer          = req.LdaServer;
        agency.Address            = req.Address;
        agency.Phone              = req.Phone;
        agency.Fax                = req.Fax;
        agency.Email              = req.Email;
        agency.Notes              = req.Notes;
        agency.SortOrder          = req.SortOrder;
        agency.ReportDisplayOrder = req.ReportDisplayOrder;

        db.GovernmentAgencies.Add(agency);
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { agency.Id }));
    }

    // ── PUT update ────────────────────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGovernmentAgencyRequest req,
        CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var agency = await db.GovernmentAgencies
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (agency is null)
            return NotFound(new { error = $"GovernmentAgency '{id}' not found." });

        // Guard duplicate code (exclude self)
        if (agency.Code != req.Code &&
            await db.GovernmentAgencies.AnyAsync(
                x => x.TenantId == tenantId && x.Code == req.Code && x.Id != id, ct))
            return Conflict(new { error = $"Code '{req.Code}' already exists." });

        // Validate parentId — must exist, no self-reference, no circular ancestry
        if (req.ParentId.HasValue)
        {
            if (req.ParentId.Value == id)
                return BadRequest(new { error = "An agency cannot be its own parent." });

            // Walk ancestors to detect indirect cycles (A→B→C→A)
            var visited = new HashSet<Guid> { id };
            var currentParent = req.ParentId;
            while (currentParent.HasValue)
            {
                if (!visited.Add(currentParent.Value))
                    return BadRequest(new { error = "Circular reference detected." });
                currentParent = await db.GovernmentAgencies
                    .Where(x => x.TenantId == tenantId && x.Id == currentParent.Value)
                    .Select(x => x.ParentId)
                    .FirstOrDefaultAsync(ct);
            }
        }

        agency.Name              = req.Name;
        agency.Code              = req.Code;
        agency.ParentId          = req.ParentId;
        agency.AgencyType        = req.AgencyType;
        agency.Origin            = req.Origin;
        agency.LdaServer         = req.LdaServer;
        agency.Address           = req.Address;
        agency.Phone             = req.Phone;
        agency.Fax               = req.Fax;
        agency.Email             = req.Email;
        agency.Notes             = req.Notes;
        agency.SortOrder         = req.SortOrder;
        agency.ReportDisplayOrder = req.ReportDisplayOrder;
        agency.IsActive          = req.IsActive;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── DELETE soft delete ────────────────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var agency = await db.GovernmentAgencies
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (agency is null)
            return NotFound(new { error = $"GovernmentAgency '{id}' not found." });

        agency.IsActive = false;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public sealed record CreateGovernmentAgencyRequest(
    string Name, string Code, Guid? ParentId, string? AgencyType,
    string? Origin, string? LdaServer, string? Address,
    string? Phone, string? Fax, string? Email, string? Notes,
    int SortOrder = 0, int? ReportDisplayOrder = null);

public sealed record UpdateGovernmentAgencyRequest(
    string Name, string Code, Guid? ParentId, string? AgencyType,
    string? Origin, string? LdaServer, string? Address,
    string? Phone, string? Fax, string? Email, string? Notes,
    int SortOrder = 0, int? ReportDisplayOrder = null, bool IsActive = true);
