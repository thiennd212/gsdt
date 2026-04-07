
namespace GSDT.SystemParams.Controllers;

/// <summary>Admin CRUD for system parameters. Requires Admin role.</summary>
[ApiController]
[Route("api/v1/admin/system-params")]
[Authorize(Roles = "Admin")]
public class SystemParamsController(
    SystemParamsDbContext db,
    ISystemParamService paramService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? tenantId, CancellationToken ct)
    {
        var query = db.SystemParameters.AsNoTracking()
            .Where(p => p.TenantId == null || p.TenantId == tenantId)
            .OrderBy(p => p.Key);

        var items = await query.Select(p => new
        {
            p.Key, p.Value, DataType = p.DataType.ToString(),
            p.Description, p.IsEditable, p.TenantId
        }).ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetOne(string key, [FromQuery] string? tenantId, CancellationToken ct)
    {
        var normalizedKey = key.ToLowerInvariant();
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
    public async Task<IActionResult> Update(
        string key, [FromBody] UpdateSystemParamRequest req,
        [FromQuery] string? tenantId, CancellationToken ct)
    {
        var normalizedKey = key.ToLowerInvariant();
        var param = await db.SystemParameters
            .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == tenantId, ct);

        if (param is null)
            return NotFound(ApiResponse<object>.Fail(
                [new FluentResults.Error($"Parameter '{normalizedKey}' not found.")]));

        try
        {
            param.UpdateValue(req.Value);
            await db.SaveChangesAsync(ct);

            // Invalidate ICacheService key
            await paramService.SetAsync(normalizedKey, req.Value, ct);
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
