
namespace GSDT.Integration.Domain.Entities;

/// <summary>
/// Immutable integration message log — records inbound/outbound message exchanges with partners.
/// State transitions: Sent → Delivered | Failed; Sent | Delivered → Acknowledged.
/// </summary>
public sealed class MessageLog : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }
    public Guid PartnerId { get; private set; }
    public Guid? ContractId { get; private set; }
    public MessageDirection Direction { get; private set; }
    public string MessageType { get; private set; } = string.Empty;
    public string? Payload { get; private set; }
    public MessageLogStatus Status { get; private set; } = MessageLogStatus.Sent;
    public string? CorrelationId { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }

    private MessageLog() { } // EF Core

    public static MessageLog Create(Guid tenantId, Guid partnerId, Guid? contractId,
        MessageDirection direction, string messageType, string? payload,
        string? correlationId, Guid createdBy)
    {
        var log = new MessageLog
        {
            Id = Guid.NewGuid(), TenantId = tenantId,
            PartnerId = partnerId, ContractId = contractId,
            Direction = direction, MessageType = messageType,
            Payload = payload, CorrelationId = correlationId,
            Status = MessageLogStatus.Sent, SentAt = DateTime.UtcNow
        };
        log.SetAuditCreate(createdBy);
        return log;
    }

    public void MarkDelivered()
    {
        if (Status != MessageLogStatus.Sent)
            throw new InvalidOperationException($"Cannot mark delivered from {Status} state.");
        Status = MessageLogStatus.Delivered;
    }

    public void MarkFailed()
    {
        if (Status is MessageLogStatus.Acknowledged)
            throw new InvalidOperationException("Cannot fail an acknowledged message.");
        Status = MessageLogStatus.Failed;
    }

    public void Acknowledge()
    {
        if (Status is not (MessageLogStatus.Sent or MessageLogStatus.Delivered))
            throw new InvalidOperationException($"Cannot acknowledge from {Status} state.");
        Status = MessageLogStatus.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
    }
}
