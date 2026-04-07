using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.CreateDocumentTemplate;

/// <summary>Creates a new DocumentTemplate in Draft status.</summary>
public sealed record CreateDocumentTemplateCommand(
    string Name,
    string Code,
    string? Description,
    DocumentOutputFormat OutputFormat,
    string TemplateContent,
    Guid TenantId,
    Guid CreatedBy) : IRequest<Result<DocumentTemplateDto>>;
