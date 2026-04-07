
namespace GSDT.Integration.Domain.Events;

public abstract record IntegrationDomainEventBase : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PartnerCreatedEvent(Guid PartnerId, Guid TenantId) : IntegrationDomainEventBase;
public sealed record PartnerDeactivatedEvent(Guid PartnerId, Guid TenantId) : IntegrationDomainEventBase;
