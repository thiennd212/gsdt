namespace GSDT.MasterData.Controllers;

/// <summary>
/// CRUD endpoints for Kế hoạch lựa chọn nhà thầu (ContractorSelectionPlan / KHLCNT).
/// Route: api/v1/masterdata/contractor-selection-plans
/// OrderNumber is auto-incremented per tenant on creation.
/// </summary>
[ApiController]
[Route("api/v1/masterdata/contractor-selection-plans")]
[Authorize]
public class ContractorSelectionPlansController(MasterDataDbContext db) : ApiControllerBase
{
    // ── GET list ──────────────────────────────────────────────────────────────

    /// <summary>List all contractor selection plans for the calling tenant.</summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var tenantId = ResolveTenantId();

        var query = db.ContractorSelectionPlans.AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        var result = await query
            .OrderBy(x => x.OrderNumber)
            .Select(x => new
            {
                x.Id, x.OrderNumber, x.NameVi, x.NameEn,
                x.SignedDate, x.SignedBy, x.IsActive
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(result));
    }

    // ── GET by ID ─────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var item = await db.ContractorSelectionPlans.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == id)
            .Select(x => new
            {
                x.Id, x.OrderNumber, x.NameVi, x.NameEn,
                x.SignedDate, x.SignedBy, x.IsActive
            })
            .FirstOrDefaultAsync(ct);

        return item is null
            ? NotFound(new { error = $"ContractorSelectionPlan '{id}' not found." })
            : Ok(ApiResponse<object>.Ok(item));
    }

    // ── POST create ───────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateContractorSelectionPlanRequest req,
        CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var plan = ContractorSelectionPlan.Create(
            tenantId, req.NameVi, req.NameEn, req.SignedDate, req.SignedBy);

        // Auto-increment OrderNumber per tenant — retry on unique constraint violation (race condition)
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var maxOrder = await db.ContractorSelectionPlans
                .Where(x => x.TenantId == tenantId)
                .MaxAsync(x => (int?)x.OrderNumber, ct) ?? 0;
            plan.OrderNumber = maxOrder + 1;

            db.ContractorSelectionPlans.Add(plan);
            try
            {
                await db.SaveChangesAsync(ct);
                return Ok(ApiResponse<object>.Ok(new { plan.Id, plan.OrderNumber }));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException) when (attempt < 2)
            {
                db.Entry(plan).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
        }

        return Conflict(ApiResponse<object>.Fail([new FluentResults.Error("GOV_MD_409: Khong the tao so thu tu, vui long thu lai.")]));
    }

    // ── PUT update ────────────────────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateContractorSelectionPlanRequest req,
        CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var plan = await db.ContractorSelectionPlans
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (plan is null)
            return NotFound(new { error = $"ContractorSelectionPlan '{id}' not found." });

        plan.NameVi     = req.NameVi;
        plan.NameEn     = req.NameEn;
        plan.SignedDate = req.SignedDate;
        plan.SignedBy   = req.SignedBy;
        plan.IsActive   = req.IsActive;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── DELETE soft delete ────────────────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var plan = await db.ContractorSelectionPlans
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (plan is null)
            return NotFound(new { error = $"ContractorSelectionPlan '{id}' not found." });

        plan.IsActive = false;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public sealed record CreateContractorSelectionPlanRequest(
    string NameVi, string NameEn, DateTime SignedDate, string SignedBy);

public sealed record UpdateContractorSelectionPlanRequest(
    string NameVi, string NameEn, DateTime SignedDate, string SignedBy, bool IsActive = true);
