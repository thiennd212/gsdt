using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Commands.CreateNotification;

public sealed class CreateNotificationCommandHandler(
    INotificationRepository repository,
    ICurrentUser currentUser) : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var channel = NotificationChannel.From(request.Channel);

        var notification = Notification.Create(
            request.TenantId,
            request.RecipientUserId,
            request.Subject,
            request.Body,
            channel,
            currentUser.UserId);

        await repository.AddAsync(notification, cancellationToken);

        return Result.Ok(notification.Id);
    }
}
