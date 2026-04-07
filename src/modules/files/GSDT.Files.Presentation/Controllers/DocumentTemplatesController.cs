using MediatR;

namespace GSDT.Files.Presentation.Controllers;

/// <summary>
/// Document Templates REST API — create, update, publish, list.
/// Templates are tenant-scoped; management requires Admin role.
/// TenantId and user identity are resolved from JWT — never accepted from clients.
/// </summary>
[Route("api/v1/document-templates")]
[Authorize]
public sealed class DocumentTemplatesController(ISender mediator) : ApiControllerBase
{
    /// <summary>List document templates for the resolved tenant.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] DocumentTemplateStatus? status = null,
        [FromQuery][StringLength(200)] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new GetDocumentTemplatesQuery(ResolveTenantId(), status, search, page, pageSize),
            cancellationToken));
    }

    /// <summary>Create a new document template in Draft status.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDocumentTemplateRequest request,
        CancellationToken cancellationToken = default) =>
        ToApiResponse(await mediator.Send(
            new CreateDocumentTemplateCommand(
                request.Name,
                request.Code,
                request.Description,
                request.OutputFormat,
                request.TemplateContent,
                ResolveTenantId(),
                ResolveUserId()),
            cancellationToken));

    /// <summary>Update template name, description, and content (creates version snapshot).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTemplateRequest request,
        CancellationToken cancellationToken = default) =>
        ToApiResponse(await mediator.Send(
            new UpdateDocumentTemplateCommand(
                id, request.Name, request.Description,
                request.TemplateContent, ResolveUserId(), ResolveTenantId()),
            cancellationToken));

    /// <summary>Publish a template (Draft → Active).</summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Publish(
        Guid id,
        CancellationToken cancellationToken = default) =>
        ToApiResponse(await mediator.Send(
            new PublishDocumentTemplateCommand(id, ResolveUserId(), ResolveTenantId()),
            cancellationToken));
}

public sealed record CreateDocumentTemplateRequest(
    string Name,
    string Code,
    string? Description,
    DocumentOutputFormat OutputFormat,
    string TemplateContent);

public sealed record UpdateTemplateRequest(
    string Name,
    string? Description,
    string TemplateContent);
