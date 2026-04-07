
namespace GSDT.MasterData.Presentation.Controllers;

/// <summary>
/// Read-only master data endpoints — Province/District/Ward from IMemoryCache,
/// CaseType/JobTitle from DB with OutputCache 30min.
/// TenantId resolved from JWT (ITenantContext) — never from query params.
/// </summary>
[ApiController]
[Route("api/v1/masterdata")]
[Authorize]
public class MasterDataController(
    IMemoryCache memoryCache,
    MasterDataDbContext db,
    IAdministrativeUnitService adminUnitService,
    ITenantContext tenantContext) : ApiControllerBase
{
    [HttpGet("provinces")]
    [OutputCache(PolicyName = "MasterData")]
    [AllowAnonymous]
    public IActionResult GetProvinces()
    {
        var provinces = memoryCache.Get<List<Province>>("masterdata:provinces") ?? [];
        return Ok(ApiResponse<IReadOnlyList<object>>.Ok(
            provinces.Where(p => p.IsActive)
                     .Select(p => new { p.Code, p.NameVi, p.NameEn, p.SortOrder })
                     .ToList<object>()));
    }

    [HttpGet("provinces/{provinceCode}/districts")]
    [OutputCache(PolicyName = "MasterData")]
    [AllowAnonymous]
    public IActionResult GetDistricts(string provinceCode)
    {
        var districts = memoryCache.Get<List<District>>("masterdata:districts") ?? [];
        var result = districts
            .Where(d => d.ProvinceCode == provinceCode && d.IsActive)
            .Select(d => new { d.Code, d.NameVi, d.NameEn, d.SortOrder })
            .ToList();
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("provinces/{provinceCode}/districts/{districtCode}/wards")]
    [OutputCache(PolicyName = "MasterData")]
    [AllowAnonymous]
    public IActionResult GetWards(string provinceCode, string districtCode)
    {
        var wards = memoryCache.Get<List<Ward>>("masterdata:wards") ?? [];
        var result = wards
            .Where(w => w.DistrictCode == districtCode && w.IsActive)
            .Select(w => new { w.Code, w.NameVi, w.NameEn, w.SortOrder })
            .ToList();
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("districts/{districtCode}/wards")]
    [OutputCache(PolicyName = "MasterData")]
    [AllowAnonymous]
    public IActionResult GetWardsByDistrict(string districtCode)
    {
        var wards = memoryCache.Get<List<Ward>>("masterdata:wards") ?? [];
        var result = wards
            .Where(w => w.DistrictCode == districtCode && w.IsActive)
            .Select(w => new { w.Code, w.NameVi, w.NameEn, w.SortOrder })
            .ToList();
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("admin-units")]
    [OutputCache(PolicyName = "AdminUnits")]
    [AllowAnonymous]
    public IActionResult GetAdminUnits([FromQuery] int? level, [FromQuery] string? parentCode)
    {
        var units = adminUnitService.GetByLevel(level ?? 1, parentCode);
        return Ok(ApiResponse<object>.Ok(
            units.Select(u => new
            {
                u.Code, u.NameVi, u.NameEn, u.Level,
                u.ParentCode, u.IsActive, u.EffectiveTo
            }).ToList()));
    }

    [HttpGet("admin-units/{code}/successor")]
    [AllowAnonymous]
    public IActionResult GetSuccessor(string code)
    {
        var activeCode = adminUnitService.ResolveActive(code);
        return Ok(ApiResponse<object>.Ok(new { requestedCode = code, activeCode }));
    }

    [HttpGet("case-types")]
    [OutputCache(PolicyName = "MasterData")]
    public async Task<IActionResult> GetCaseTypes(CancellationToken ct)
    {
        // TenantId from JWT — never from query param (GOV_SEC: tenant isolation)
        var tid = tenantContext.TenantId?.ToString();
        var result = await db.CaseTypes.AsNoTracking()
            .Where(c => c.IsActive && (c.TenantId == null || c.TenantId == tid))
            .OrderBy(c => c.SortOrder)
            .Select(c => new { c.Code, c.NameVi, c.NameEn })
            .ToListAsync(ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("job-titles")]
    [OutputCache(PolicyName = "MasterData")]
    public async Task<IActionResult> GetJobTitles(CancellationToken ct)
    {
        var tid = tenantContext.TenantId?.ToString();
        var result = await db.JobTitles.AsNoTracking()
            .Where(j => j.IsActive && (j.TenantId == null || j.TenantId == tid))
            .OrderBy(j => j.SortOrder)
            .Select(j => new { j.Code, j.NameVi, j.NameEn })
            .ToListAsync(ct);
        return Ok(ApiResponse<object>.Ok(result));
    }
}
