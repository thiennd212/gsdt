using FluentValidation;

namespace GSDT.SystemParams.Controllers;

/// <summary>
/// Admin: full CRUD for system announcements.
/// Public: GET /api/v1/announcements/active — active announcements, cached 1min.
/// </summary>
[ApiController]
public class AnnouncementsController(
    SystemParamsDbContext db,
    ICacheService cache,
    IValidator<CreateAnnouncementRequest> createValidator) : ApiControllerBase
{
    private const string ActiveCacheKey = "announcements:active";
    private static readonly TimeSpan ActiveCacheTtl = TimeSpan.FromMinutes(1);

    // --- Admin endpoints ---

    [HttpGet("api/v1/admin/announcements")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await db.SystemAnnouncements.AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id, a.Title, a.Content,
                Severity = a.Severity.ToString(),
                a.StartAt, a.EndAt, a.IsActive, a.TargetRoles
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPost("api/v1/admin/announcements")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAnnouncementRequest req, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return UnprocessableEntity(ApiResponse<object>.Fail(
                validation.Errors.Select(e => new FluentResults.Error(e.ErrorMessage)).Cast<FluentResults.IError>().ToList()));

        var announcement = SystemAnnouncement.Create(
            req.Title, req.Content, req.Severity,
            req.StartAt, req.EndAt, req.TargetRoles);

        db.SystemAnnouncements.Add(announcement);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(ActiveCacheKey, ct);

        return Ok(ApiResponse<object>.Ok(new { announcement.Id }));
    }

    [HttpPut("api/v1/admin/announcements/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateAnnouncementRequest req, CancellationToken ct)
    {
        var announcement = await db.SystemAnnouncements.FindAsync([id], ct);
        if (announcement is null)
            return NotFound(ApiResponse<object>.Fail(
                [new FluentResults.Error($"Announcement {id} not found.")]));

        announcement.Update(req.Title, req.Content, req.Severity,
            req.StartAt, req.EndAt, req.IsActive);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(ActiveCacheKey, ct);

        return Ok(ApiResponse<object>.Ok(new { id }));
    }

    [HttpDelete("api/v1/admin/announcements/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var announcement = await db.SystemAnnouncements.FindAsync([id], ct);
        if (announcement is null)
            return NotFound(ApiResponse<object>.Fail(
                [new FluentResults.Error($"Announcement {id} not found.")]));

        announcement.Deactivate();
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(ActiveCacheKey, ct);

        return NoContent();
    }

    // --- Public endpoint ---

    [HttpGet("api/v1/announcements/active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var cached = await cache.GetAsync<List<object>>(ActiveCacheKey, ct);
        if (cached is not null)
            return Ok(ApiResponse<object>.Ok(cached));

        var now = DateTimeOffset.UtcNow;
        var active = await db.SystemAnnouncements.AsNoTracking()
            .Where(a => a.IsActive
                && (a.StartAt == null || a.StartAt <= now)
                && (a.EndAt == null || a.EndAt >= now))
            .OrderByDescending(a => a.Severity)
            .Select(a => (object)new
            {
                a.Id, a.Title, a.Content,
                Severity = a.Severity.ToString(),
                a.StartAt, a.EndAt, a.TargetRoles
            })
            .ToListAsync(ct);

        await cache.SetAsync(ActiveCacheKey, active, ActiveCacheTtl, ct);
        return Ok(ApiResponse<object>.Ok(active));
    }
}

public record CreateAnnouncementRequest(
    string Title,
    string Content,
    AnnouncementSeverity Severity,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    string? TargetRoles);

public record UpdateAnnouncementRequest(
    string Title,
    string Content,
    AnnouncementSeverity Severity,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    bool IsActive);

/// <summary>FluentValidation validator for CreateAnnouncementRequest.</summary>
public sealed class CreateAnnouncementRequestValidator : AbstractValidator<CreateAnnouncementRequest>
{
    public CreateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(200).WithMessage("Tiêu đề không vượt quá 200 ký tự.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung không được để trống.")
            .MaximumLength(2000).WithMessage("Nội dung không vượt quá 2000 ký tự.");
        RuleFor(x => x.Severity)
            .IsInEnum().WithMessage("Mức độ thông báo không hợp lệ.");
        // If both dates provided, start must be before end
        When(x => x.StartAt.HasValue && x.EndAt.HasValue, () =>
        {
            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithMessage("Ngày kết thúc phải sau ngày bắt đầu.");
        });
    }
}
