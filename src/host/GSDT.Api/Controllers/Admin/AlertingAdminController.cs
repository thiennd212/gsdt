
namespace GSDT.Api.Controllers.Admin;

/// <summary>
/// Admin CRUD for AlertRules (M14 Observability).
/// POST /api/v1/admin/alerts/{id}/test triggers an immediate evaluation of a single rule.
/// Requires SystemAdmin role.
/// </summary>
[Route("api/v1/admin/alerts")]
[Authorize(Roles = "SystemAdmin")]
[EnableRateLimiting("write-ops")]
public sealed class AlertingAdminController(
    AlertingDbContext db,
    AlertEvaluationJob evaluationJob) : ApiControllerBase
{
    // =========================================================================
    // Alert Rules
    // =========================================================================

    /// <summary>List all alert rules with last-trigger info, most-recently-modified first.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ListRules(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(page, 1) - 1) * pageSize;

        var total = await db.AlertRules.CountAsync(ct);
        var items = await db.AlertRules
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .Skip(skip).Take(pageSize)
            .Select(r => new AlertRuleDto(
                r.Id, r.Name, r.MetricName, r.Condition, r.Threshold,
                r.WindowMinutes, r.NotifyChannel, r.NotifyTarget,
                r.Enabled, r.LastTriggeredAt, r.ConsecutiveBreaches))
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(new { items, totalCount = total }));
    }

    /// <summary>Get a single alert rule by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRule(Guid id, CancellationToken ct)
    {
        var rule = await db.AlertRules.FindAsync([id], ct);
        if (rule is null) return NotFound();

        return Ok(ApiResponse<AlertRuleDto>.Ok(new AlertRuleDto(
            rule.Id, rule.Name, rule.MetricName, rule.Condition, rule.Threshold,
            rule.WindowMinutes, rule.NotifyChannel, rule.NotifyTarget,
            rule.Enabled, rule.LastTriggeredAt, rule.ConsecutiveBreaches)));
    }

    /// <summary>Create a new alert rule.</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> CreateRule(
        [FromBody] UpsertAlertRuleRequest req,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var rule = AlertRule.Create(
            req.Name, req.MetricName, req.Condition, req.Threshold,
            req.WindowMinutes, req.NotifyChannel, req.NotifyTarget, userId);

        db.AlertRules.Add(rule);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetRule), new { id = rule.Id },
            ApiResponse<object>.Ok(new { id = rule.Id }));
    }

    /// <summary>Update an existing alert rule.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateRule(
        Guid id,
        [FromBody] UpsertAlertRuleRequest req,
        CancellationToken ct)
    {
        var rule = await db.AlertRules.FindAsync([id], ct);
        if (rule is null) return NotFound();

        rule.Update(req.Name, req.MetricName, req.Condition, req.Threshold,
            req.WindowMinutes, req.NotifyChannel, req.NotifyTarget, GetUserId());

        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { id = rule.Id }));
    }

    /// <summary>Soft-delete an alert rule.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteRule(Guid id, CancellationToken ct)
    {
        var rule = await db.AlertRules.FindAsync([id], ct);
        if (rule is null) return NotFound();

        rule.Delete();
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Trigger an immediate test evaluation for one alert rule.</summary>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType(202)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> TestRule(Guid id, CancellationToken ct)
    {
        var exists = await db.AlertRules.AnyAsync(r => r.Id == id, ct);
        if (!exists) return NotFound();

        // Run the full evaluation cycle — test evaluates all enabled rules including this one
        await evaluationJob.ExecuteAsync(ct);

        return Accepted(ApiResponse<object>.Ok(new { message = "Test evaluation triggered." }));
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private Guid GetUserId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

// ---- Request / Response DTOs ------------------------------------------------

public sealed record UpsertAlertRuleRequest(
    string Name,
    string MetricName,
    string Condition,
    double Threshold,
    int WindowMinutes,
    string NotifyChannel,
    string? NotifyTarget);

public sealed record AlertRuleDto(
    Guid Id,
    string Name,
    string MetricName,
    string Condition,
    double Threshold,
    int WindowMinutes,
    string NotifyChannel,
    string? NotifyTarget,
    bool Enabled,
    DateTime? LastTriggeredAt,
    int ConsecutiveBreaches);

