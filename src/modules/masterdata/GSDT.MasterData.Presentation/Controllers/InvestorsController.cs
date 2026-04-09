
namespace GSDT.MasterData.Presentation.Controllers;

/// <summary>
/// CRUD endpoints for Chủ đầu tư (Investor).
/// Route: api/v1/masterdata/investors
/// Flat tenant-scoped catalog. Filter by investorType query param.
/// TenantId resolved from JWT via ResolveTenantId() — never from query params.
/// </summary>
[ApiController]
[Route("api/v1/masterdata/investors")]
[Authorize(Roles = "BTC,CQCQ,CDT")]
public class InvestorsController(MasterDataDbContext db) : ApiControllerBase
{
    // ── GET list ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? investorType = null,
        CancellationToken ct = default)
    {
        var tenantId = ResolveTenantId();

        var query = db.Investors.AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(investorType))
            query = query.Where(x => x.InvestorType == investorType);

        var result = await query
            .OrderBy(x => x.NameVi)
            .Select(x => new
            {
                x.Id, x.InvestorType, x.BusinessIdOrCccd,
                x.NameVi, x.NameEn, x.IsActive
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(result));
    }

    // ── GET by ID ─────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var item = await db.Investors.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == id)
            .Select(x => new
            {
                x.Id, x.InvestorType, x.BusinessIdOrCccd,
                x.NameVi, x.NameEn, x.IsActive
            })
            .FirstOrDefaultAsync(ct);

        return item is null
            ? NotFound(new { error = $"Investor '{id}' not found." })
            : Ok(ApiResponse<object>.Ok(item));
    }

    // ── POST create ───────────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvestorRequest req,
        CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        if (await db.Investors.AnyAsync(
                x => x.TenantId == tenantId && x.BusinessIdOrCccd == req.BusinessIdOrCccd, ct))
            return Conflict(new { error = $"BusinessIdOrCccd '{req.BusinessIdOrCccd}' already exists." });

        var investor = Investor.Create(
            tenantId, req.InvestorType, req.BusinessIdOrCccd, req.NameVi, req.NameEn);

        db.Investors.Add(investor);
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { investor.Id }));
    }

    // ── PUT update ────────────────────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateInvestorRequest req,
        CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var investor = await db.Investors
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (investor is null)
            return NotFound(new { error = $"Investor '{id}' not found." });

        if (investor.BusinessIdOrCccd != req.BusinessIdOrCccd &&
            await db.Investors.AnyAsync(
                x => x.TenantId == tenantId && x.BusinessIdOrCccd == req.BusinessIdOrCccd && x.Id != id, ct))
            return Conflict(new { error = $"BusinessIdOrCccd '{req.BusinessIdOrCccd}' already exists." });

        investor.InvestorType     = req.InvestorType;
        investor.BusinessIdOrCccd = req.BusinessIdOrCccd;
        investor.NameVi           = req.NameVi;
        investor.NameEn           = req.NameEn;
        investor.IsActive         = req.IsActive;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── DELETE soft delete ────────────────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "BTC")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();

        var investor = await db.Investors
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (investor is null)
            return NotFound(new { error = $"Investor '{id}' not found." });

        investor.IsActive = false;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public sealed record CreateInvestorRequest(
    string InvestorType, string BusinessIdOrCccd, string NameVi, string? NameEn);

public sealed record UpdateInvestorRequest(
    string InvestorType, string BusinessIdOrCccd, string NameVi, string? NameEn, bool IsActive = true);
