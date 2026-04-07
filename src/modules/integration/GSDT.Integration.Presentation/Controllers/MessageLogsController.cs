using MediatR;

namespace GSDT.Integration.Presentation.Controllers;

/// <summary>
/// Integration Message Log REST API — audit trail for partner message exchanges.
/// No DELETE endpoint — message logs are immutable audit records.
/// TenantId resolved from JWT claim (ITenantContext) — never caller-supplied.
/// </summary>
[Route("api/v1/integration/message-logs")]
[Authorize]
public sealed class MessageLogsController(ISender mediator, ITenantContext tenantContext)
    : ApiControllerBase
{
    /// <summary>List message logs, optionally filtered by partnerId and/or contractId. pageSize clamped to [1, 100].</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? partnerId,
        [FromQuery] Guid? contractId,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new ListMessageLogsQuery(tenantId, partnerId, contractId, search, page, pageSize), ct));
    }

    /// <summary>Get message log detail by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        return ToApiResponse(await mediator.Send(
            new GetMessageLogQuery(id, tenantId), ct));
    }

    /// <summary>Log a new message exchange record.</summary>
    [HttpPost]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMessageLogBody body, CancellationToken ct)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        return ToApiResponse(await mediator.Send(
            new CreateMessageLogCommand(tenantId, body.PartnerId, body.ContractId,
                body.Direction, body.MessageType, body.Payload,
                body.CorrelationId), ct));
    }

    /// <summary>Update message log delivery status (Delivered, Failed, Acknowledged).</summary>
    [HttpPut("{id:guid}/status")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateMessageLogStatusBody body, CancellationToken ct) =>
        ToApiResponse(await mediator.Send(
            new UpdateMessageLogStatusCommand(id, body.NewStatus), ct));
}

// Request body records — lightweight, no entity types (GOV_SEC_004 mass-assignment prevention)
public sealed record CreateMessageLogBody(
    Guid PartnerId, Guid? ContractId,
    MessageDirection Direction, string MessageType,
    string? Payload, string? CorrelationId);

public sealed record UpdateMessageLogStatusBody(MessageLogStatus NewStatus);
