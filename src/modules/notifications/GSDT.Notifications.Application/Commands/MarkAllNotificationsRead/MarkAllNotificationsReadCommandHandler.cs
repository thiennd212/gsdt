using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler(INotificationRepository repository)
    : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    public async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await repository.GetUnreadByUserAsync(request.UserId, request.TenantId, cancellationToken);
        foreach (var notification in unread)
        {
            notification.MarkAsRead();
            await repository.UpdateAsync(notification, cancellationToken);
        }
        return Result.Ok();
    }
}
