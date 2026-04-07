using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.PublishDocumentTemplate;

/// <summary>Publishes a DocumentTemplate (Draft → Active).</summary>
public sealed record PublishDocumentTemplateCommand(
    Guid TemplateId,
    Guid PublishedBy,
    Guid TenantId) : IRequest<Result<DocumentTemplateDto>>;
