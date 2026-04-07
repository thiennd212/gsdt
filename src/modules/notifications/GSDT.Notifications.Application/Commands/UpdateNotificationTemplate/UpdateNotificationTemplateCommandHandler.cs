using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Commands.UpdateNotificationTemplate;

public sealed class UpdateNotificationTemplateCommandHandler(
    INotificationTemplateRepository repository,
    ICurrentUser currentUser) : IRequestHandler<UpdateNotificationTemplateCommand, Result>
{
    public async Task<Result> Handle(
        UpdateNotificationTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (result.IsFailed)
            return Result.Fail(new NotFoundError($"Notification template {request.Id} not found."));

        result.Value.Update(request.SubjectTemplate, request.BodyTemplate, currentUser.UserId);
        await repository.UpdateAsync(result.Value, cancellationToken);
        return Result.Ok();
    }
}
