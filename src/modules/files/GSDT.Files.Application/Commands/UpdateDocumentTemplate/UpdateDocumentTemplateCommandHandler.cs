using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.UpdateDocumentTemplate;

public sealed class UpdateDocumentTemplateCommandHandler(
    IRepository<DocumentTemplate, Guid> templateRepository)
    : IRequestHandler<UpdateDocumentTemplateCommand, Result<DocumentTemplateDto>>
{
    public async Task<Result<DocumentTemplateDto>> Handle(
        UpdateDocumentTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var result = await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (result.IsFailed)
            return Result.Fail(new NotFoundError($"DocumentTemplate '{request.TemplateId}' not found."));

        var template = result.Value;

        template.Update(request.Name, request.Description, request.TemplateContent, request.ModifiedBy);
        await templateRepository.UpdateAsync(template, cancellationToken);

        return Result.Ok(CreateDocumentTemplateCommandHandler.MapToDto(template));
    }
}
