namespace GSDT.Notifications.Application.DTOs;

public sealed record NotificationDto(
    Guid Id,
    Guid TenantId,
    Guid RecipientUserId,
    string Subject,
    string Body,
    string Channel,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt);
