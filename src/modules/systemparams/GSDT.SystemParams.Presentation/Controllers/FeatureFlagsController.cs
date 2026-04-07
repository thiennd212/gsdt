
namespace GSDT.SystemParams.Presentation.Controllers;

/// <summary>
/// Admin: manage feature flags (SystemParameter with key prefix "feature:").
/// Public: GET /api/v1/feature-flags/{key} — sync IsEnabled check from L0 ConcurrentDict.
/// </summary>
[ApiController]
public class FeatureFlagsController(
    SystemParamsDbContext db,
    ISystemParamService paramService,
    IFeatureFlagService featureFlagService) : ApiControllerBase
{
    [HttpGet("api/v1/admin/feature-flags")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var flags = await db.SystemParameters.AsNoTracking()
            .Where(p => p.Key.StartsWith("feature:"))
            .OrderBy(p => p.Key)
            .Select(p => new
            {
                Name = p.Key.Substring("feature:".Length),
                Key = p.Key,
                IsEnabled = p.Value == "true",
                p.Description,
                p.TenantId
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(flags));
    }

    [HttpPut("api/v1/admin/feature-flags/{flagName}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write-ops")]
    public async Task<IActionResult> Toggle(
        string flagName, [FromBody] ToggleFeatureFlagRequest req,
        CancellationToken ct)
    {
        var key = $"feature:{flagName.ToLowerInvariant()}";
        var tenantId = ResolveTenantId().ToString();
        await paramService.SetAsync(key, req.IsEnabled, tenantId, ct);
        return Ok(ApiResponse<object>.Ok(new { key, isEnabled = req.IsEnabled }));
    }

    [HttpGet("api/v1/feature-flags/{key}")]
    [AllowAnonymous]
    public IActionResult IsEnabled(string key, [FromQuery] string? tenantId)
    {
        var flagKey = key.StartsWith("feature:", StringComparison.OrdinalIgnoreCase)
            ? key : $"feature:{key}";
        var enabled = featureFlagService.IsEnabled(flagKey, tenantId);
        return Ok(ApiResponse<object>.Ok(new { key = flagKey, enabled }));
    }
}

public record ToggleFeatureFlagRequest(bool IsEnabled);
