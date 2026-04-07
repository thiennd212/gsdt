using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Commands.DeleteNotificationTemplate;

public sealed class DeleteNotificationTemplateCommandHandler(
    INotificationTemplateRepository repository) : IRequestHandler<DeleteNotificationTemplateCommand, Result>
{
    public async Task<Result> Handle(
        DeleteNotificationTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (result.IsFailed)
            return Result.Fail(new NotFoundError($"Notification template {request.Id} not found."));

        await repository.DeleteAsync(request.Id, cancellationToken);
        return Result.Ok();
    }
}
