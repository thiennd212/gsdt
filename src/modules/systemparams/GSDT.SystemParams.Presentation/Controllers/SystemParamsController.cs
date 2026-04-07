
namespace GSDT.SystemParams.Presentation.Controllers;

/// <summary>Admin CRUD for system parameters. Requires Admin role.</summary>
[ApiController]
[Route("api/v1/admin/system-params")]
[Authorize(Roles = "Admin")]
public class SystemParamsController(
    SystemParamsDbContext db,
    ISystemParamService paramService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = ResolveTenantId().ToString();
        var items = await db.SystemParameters.AsNoTracking()
            .Where(p => p.TenantId == null || p.TenantId == tenantId)
            .OrderBy(p => p.Key)
            .Select(p => new
            {
                p.Key, p.Value, DataType = p.DataType.ToString(),
                p.Description, p.IsEditable, p.TenantId
            }).ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetOne(string key, CancellationToken ct)
    {
        var normalizedKey = key.ToLowerInvariant();
        var tenantId = ResolveTenantId().ToString();
        var param = await db.SystemParameters.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == tenantId, ct);

        if (param is null)
            return NotFound(ApiResponse<object>.Fail(
                [new FluentResults.Error($"Parameter '{normalizedKey}' not found.")]));

        return Ok(ApiResponse<object>.Ok(new
        {
            param.Key, param.Value, DataType = param.DataType.ToString(),
            param.Description, param.IsEditable, param.TenantId
        }));
    }

    [HttpPut("{key}")]
    [EnableRateLimiting("write-ops")]
    public async Task<IActionResult> Update(
        string key, [FromBody] UpdateSystemParamRequest req,
        CancellationToken ct)
    {
        var normalizedKey = key.ToLowerInvariant();
        var tenantId = ResolveTenantId().ToString();
        var param = await db.SystemParameters
            .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == tenantId, ct);

        if (param is null)
            return NotFound(ApiResponse<object>.Fail(
                [new FluentResults.Error($"Parameter '{normalizedKey}' not found.")]));

        try
        {
            param.UpdateValue(req.Value);
            await db.SaveChangesAsync(ct);
            await paramService.SetAsync(normalizedKey, req.Value, tenantId, ct);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                [new FluentResults.Error(ex.Message)]));
        }

        return Ok(ApiResponse<object>.Ok(new { param.Key, param.Value }));
    }
}

public record UpdateSystemParamRequest(string Value);
