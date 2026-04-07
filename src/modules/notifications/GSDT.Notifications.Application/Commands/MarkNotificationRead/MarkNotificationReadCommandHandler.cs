using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(INotificationRepository repository)
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var result = await repository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (result.IsFailed) return Result.Fail(new NotFoundError($"Notification {request.NotificationId} not found."));

        var notification = result.Value;
        if (notification.RecipientUserId != request.UserId)
            return Result.Fail(new ForbiddenError("Cannot mark another user's notification as read."));

        notification.MarkAsRead();
        await repository.UpdateAsync(notification, cancellationToken);
        return Result.Ok();
    }
}
