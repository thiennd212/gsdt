
namespace GSDT.Notifications.Domain.Entities;

/// <summary>
/// Idempotency guard — unique index on (TemplateId, RecipientId, CorrelationId).
/// Prevents double-send on retry storms (S52 dedup spec).
/// </summary>
public sealed class NotificationLog : Entity<Guid>
{
    public Guid TemplateId { get; private set; }
    public Guid RecipientId { get; private set; }

    /// <summary>Correlation ID from originating request/event (e.g. CaseId, AuditId).</summary>
    public string CorrelationId { get; private set; } = string.Empty;

    private NotificationLog() { } // EF Core

    public static NotificationLog Create(Guid templateId, Guid recipientId, string correlationId)
    {
        return new NotificationLog
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            RecipientId = recipientId,
            CorrelationId = correlationId
        };
    }
}
