using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Commands.SendNotification;

/// <summary>
/// Sends a notification via the specified channel.
/// Dedup: if TemplateId + CorrelationId combination already logged, skips dispatch (returns existing concept).
/// Channel routing: InApp → DB insert; Email → IEmailSender; SMS → ISmsProvider.
/// </summary>
public sealed class SendNotificationCommandHandler(
    INotificationRepository notificationRepository,
    INotificationLogRepository notificationLogRepository,
    IEmailSender emailSender,
    ISmsProvider smsProvider,
    ICurrentUser currentUser) : IRequestHandler<SendNotificationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        // Dedup check: if templateId + recipientId + correlationId already sent, skip
        if (request.TemplateId.HasValue && !string.IsNullOrEmpty(request.CorrelationId))
        {
            var isDuplicate = await notificationLogRepository.ExistsAsync(
                request.TemplateId.Value, request.RecipientUserId, request.CorrelationId, cancellationToken);
            if (isDuplicate)
                return Result.Fail($"Notification already sent (template={request.TemplateId}, correlationId={request.CorrelationId}).");
        }

        var channel = NotificationChannel.From(request.Channel);

        var notification = Notification.Create(
            request.TenantId,
            request.RecipientUserId,
            request.Subject,
            request.Body,
            channel,
            currentUser.UserId,
            request.TemplateId,
            request.CorrelationId);

        await notificationRepository.AddAsync(notification, cancellationToken);

        // Dispatch per channel
        try
        {
            if (channel == NotificationChannel.Email)
                await emailSender.SendAsync(request.RecipientUserId.ToString(), request.Subject, request.Body, cancellationToken);
            else if (channel == NotificationChannel.Sms)
                await smsProvider.SendAsync(request.RecipientUserId.ToString(), request.Body, cancellationToken);
            // InApp: saved to DB above; SignalR push handled by NotificationCreatedEvent handler

            notification.MarkAsSent();
            await notificationRepository.UpdateAsync(notification, cancellationToken);
        }
        catch (Exception)
        {
            notification.MarkAsFailed();
            await notificationRepository.UpdateAsync(notification, cancellationToken);
            throw;
        }

        // Log dedup record
        if (request.TemplateId.HasValue && !string.IsNullOrEmpty(request.CorrelationId))
        {
            var log = NotificationLog.Create(request.TemplateId.Value, request.RecipientUserId, request.CorrelationId);
            await notificationLogRepository.AddAsync(log, cancellationToken);
        }

        return Result.Ok(notification.Id);
    }
}
