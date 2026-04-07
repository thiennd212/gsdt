
namespace GSDT.MasterData.Controllers;

/// <summary>
/// Admin CUD endpoints for master data (provinces, districts, wards).
/// Invalidates IMemoryCache after every write so the read controller sees fresh data immediately.
/// </summary>
[ApiController]
[Route("api/v1/admin/master-data")]
[Authorize(Roles = "Admin,SystemAdmin")]
public class MasterDataAdminController(
    MasterDataDbContext db,
    IMemoryCache memoryCache) : ControllerBase
{
    // ── Province ──────────────────────────────────────────────────────────────

    [HttpPost("provinces")]
    public async Task<IActionResult> CreateProvince(
        [FromBody] UpsertProvinceRequest req,
        CancellationToken ct)
    {
        if (await db.Provinces.AnyAsync(p => p.Code == req.Code, ct))
            return Conflict(new { error = $"Province code '{req.Code}' already exists." });

        var province = Province.Create(req.Code, req.NameVi, req.NameEn, req.SortOrder);
        db.Provinces.Add(province);
        await db.SaveChangesAsync(ct);
        InvalidateProvinceCache();
        return Ok(ApiResponse<object>.Ok(new { province.Id }));
    }

    [HttpPut("provinces/{code}")]
    public async Task<IActionResult> UpdateProvince(
        string code,
        [FromBody] UpsertProvinceRequest req,
        CancellationToken ct)
    {
        var province = await db.Provinces.FirstOrDefaultAsync(p => p.Code == code, ct);
        if (province is null) return NotFound(new { error = $"Province '{code}' not found." });

        // Entity uses private setters — update via EF shadow properties
        db.Entry(province).CurrentValues.SetValues(new
        {
            req.NameVi,
            req.NameEn,
            req.SortOrder
        });
        await db.SaveChangesAsync(ct);
        InvalidateProvinceCache();
        return NoContent();
    }

    [HttpDelete("provinces/{code}")]
    public async Task<IActionResult> DeleteProvince(string code, CancellationToken ct)
    {
        var province = await db.Provinces.FirstOrDefaultAsync(p => p.Code == code, ct);
        if (province is null) return NotFound(new { error = $"Province '{code}' not found." });

        db.Entry(province).CurrentValues["IsActive"] = false;
        await db.SaveChangesAsync(ct);
        InvalidateProvinceCache();
        return NoContent();
    }

    // ── District ─────────────────────────────────────────────────────────────

    [HttpPost("districts")]
    public async Task<IActionResult> CreateDistrict(
        [FromBody] UpsertDistrictRequest req,
        CancellationToken ct)
    {
        if (await db.Districts.AnyAsync(d => d.Code == req.Code, ct))
            return Conflict(new { error = $"District code '{req.Code}' already exists." });

        var district = District.Create(req.Code, req.ProvinceCode, req.NameVi, req.NameEn, req.SortOrder);
        db.Districts.Add(district);
        await db.SaveChangesAsync(ct);
        InvalidateDistrictCache();
        return Ok(ApiResponse<object>.Ok(new { district.Id }));
    }

    [HttpPut("districts/{code}")]
    public async Task<IActionResult> UpdateDistrict(
        string code,
        [FromBody] UpsertDistrictRequest req,
        CancellationToken ct)
    {
        var district = await db.Districts.FirstOrDefaultAsync(d => d.Code == code, ct);
        if (district is null) return NotFound(new { error = $"District '{code}' not found." });

        db.Entry(district).CurrentValues.SetValues(new
        {
            req.NameVi,
            req.NameEn,
            req.SortOrder,
            req.ProvinceCode
        });
        await db.SaveChangesAsync(ct);
        InvalidateDistrictCache();
        return NoContent();
    }

    [HttpDelete("districts/{code}")]
    public async Task<IActionResult> DeleteDistrict(string code, CancellationToken ct)
    {
        var district = await db.Districts.FirstOrDefaultAsync(d => d.Code == code, ct);
        if (district is null) return NotFound(new { error = $"District '{code}' not found." });

        db.Entry(district).CurrentValues["IsActive"] = false;
        await db.SaveChangesAsync(ct);
        InvalidateDistrictCache();
        return NoContent();
    }

    // ── Ward ──────────────────────────────────────────────────────────────────

    [HttpPost("wards")]
    public async Task<IActionResult> CreateWard(
        [FromBody] UpsertWardRequest req,
        CancellationToken ct)
    {
        if (await db.Wards.AnyAsync(w => w.Code == req.Code, ct))
            return Conflict(new { error = $"Ward code '{req.Code}' already exists." });

        var ward = Ward.Create(req.Code, req.DistrictCode, req.NameVi, req.NameEn, req.SortOrder);
        db.Wards.Add(ward);
        await db.SaveChangesAsync(ct);
        InvalidateWardCache();
        return Ok(ApiResponse<object>.Ok(new { ward.Id }));
    }

    [HttpPut("wards/{code}")]
    public async Task<IActionResult> UpdateWard(
        string code,
        [FromBody] UpsertWardRequest req,
        CancellationToken ct)
    {
        var ward = await db.Wards.FirstOrDefaultAsync(w => w.Code == code, ct);
        if (ward is null) return NotFound(new { error = $"Ward '{code}' not found." });

        db.Entry(ward).CurrentValues.SetValues(new
        {
            req.NameVi,
            req.NameEn,
            req.SortOrder,
            req.DistrictCode
        });
        await db.SaveChangesAsync(ct);
        InvalidateWardCache();
        return NoContent();
    }

    [HttpDelete("wards/{code}")]
    public async Task<IActionResult> DeleteWard(string code, CancellationToken ct)
    {
        var ward = await db.Wards.FirstOrDefaultAsync(w => w.Code == code, ct);
        if (ward is null) return NotFound(new { error = $"Ward '{code}' not found." });

        db.Entry(ward).CurrentValues["IsActive"] = false;
        await db.SaveChangesAsync(ct);
        InvalidateWardCache();
        return NoContent();
    }

    // ── Cache invalidation helpers ────────────────────────────────────────────

    private void InvalidateProvinceCache() => memoryCache.Remove("masterdata:provinces");
    private void InvalidateDistrictCache() => memoryCache.Remove("masterdata:districts");
    private void InvalidateWardCache()     => memoryCache.Remove("masterdata:wards");
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public sealed record UpsertProvinceRequest(
    string Code, string NameVi, string NameEn, int SortOrder = 0);

public sealed record UpsertDistrictRequest(
    string Code, string ProvinceCode, string NameVi, string NameEn, int SortOrder = 0);

public sealed record UpsertWardRequest(
    string Code, string DistrictCode, string NameVi, string NameEn, int SortOrder = 0);
