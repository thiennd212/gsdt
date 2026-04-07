using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.CreateDocumentTemplate;

public sealed class CreateDocumentTemplateCommandHandler(
    IRepository<DocumentTemplate, Guid> repository)
    : IRequestHandler<CreateDocumentTemplateCommand, Result<DocumentTemplateDto>>
{
    public async Task<Result<DocumentTemplateDto>> Handle(
        CreateDocumentTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = DocumentTemplate.Create(
            request.Name,
            request.Code,
            request.Description,
            request.OutputFormat,
            request.TemplateContent,
            request.TenantId,
            request.CreatedBy);

        await repository.AddAsync(template, cancellationToken);

        return Result.Ok(MapToDto(template));
    }

    internal static DocumentTemplateDto MapToDto(DocumentTemplate t) => new(
        t.Id, t.Name, t.Code, t.Description, t.OutputFormat,
        t.TemplateContent, t.Status, t.TenantId, t.CreatedAt, t.CreatedBy);
}
