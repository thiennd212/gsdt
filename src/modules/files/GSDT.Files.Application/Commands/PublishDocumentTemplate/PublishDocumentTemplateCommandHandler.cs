using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.PublishDocumentTemplate;

public sealed class PublishDocumentTemplateCommandHandler(
    IRepository<DocumentTemplate, Guid> repository)
    : IRequestHandler<PublishDocumentTemplateCommand, Result<DocumentTemplateDto>>
{
    public async Task<Result<DocumentTemplateDto>> Handle(
        PublishDocumentTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (result.IsFailed)
            return Result.Fail(new NotFoundError($"DocumentTemplate '{request.TemplateId}' not found."));

        var template = result.Value;
        template.Publish(request.PublishedBy);
        await repository.UpdateAsync(template, cancellationToken);

        return Result.Ok(CreateDocumentTemplateCommandHandler.MapToDto(template));
    }
}
