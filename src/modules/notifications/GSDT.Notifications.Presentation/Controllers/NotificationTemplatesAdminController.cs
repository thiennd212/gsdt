using MediatR;

namespace GSDT.Notifications.Presentation.Controllers;

/// <summary>
/// Admin API for managing notification templates.
/// Requires SystemAdmin or TenantAdmin role.
/// </summary>
[Authorize(Roles = "SystemAdmin,TenantAdmin")]
[Route("api/v1/admin/notification-templates")]
[EnableRateLimiting("write-ops")]
public sealed class NotificationTemplatesAdminController(ISender mediator) : ApiControllerBase
{
    /// <summary>List notification templates for a tenant.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] Guid tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? channel = null,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new GetNotificationTemplatesQuery(tenantId, page, pageSize, channel), cancellationToken));
    }

    /// <summary>Get a single notification template by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTemplateById(
        Guid id,
        CancellationToken cancellationToken = default) =>
        ToApiResponse(await mediator.Send(new GetNotificationTemplateByIdQuery(id), cancellationToken));

    /// <summary>Create a new notification template.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateNotificationTemplateCommand command,
        CancellationToken cancellationToken = default) =>
        ToApiResponse(await mediator.Send(command, cancellationToken));

    /// <summary>Update subject and body of an existing notification template.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UpdateTemplate(
        Guid id,
        [FromBody] UpdateNotificationTemplateRequest body,
        CancellationToken cancellationToken = default) =>
        ToApiResponse(await mediator.Send(
            new UpdateNotificationTemplateCommand(id, body.SubjectTemplate, body.BodyTemplate),
            cancellationToken));

    /// <summary>Soft-delete a notification template.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTemplate(
        Guid id,
        CancellationToken cancellationToken = default) =>
        ToApiResponse(await mediator.Send(new DeleteNotificationTemplateCommand(id), cancellationToken));
}

/// <summary>Request body for PUT — only mutable fields.</summary>
public sealed record UpdateNotificationTemplateRequest(string SubjectTemplate, string BodyTemplate);
