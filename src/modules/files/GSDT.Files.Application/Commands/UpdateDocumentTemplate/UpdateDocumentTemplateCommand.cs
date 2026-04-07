using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.UpdateDocumentTemplate;

/// <summary>Updates name, description, and template content. Creates a version snapshot.</summary>
public sealed record UpdateDocumentTemplateCommand(
    Guid TemplateId,
    string Name,
    string? Description,
    string TemplateContent,
    Guid ModifiedBy,
    Guid TenantId) : IRequest<Result<DocumentTemplateDto>>;
