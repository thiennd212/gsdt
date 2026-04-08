
using GSDT.SharedKernel.Domain.Events;

namespace GSDT.SharedKernel.Domain;

/// <summary>
/// Marker interface for DDD aggregate roots with domain event collection.
/// Implementors hold a mutable list of IDomainEvent instances raised during the aggregate's
/// lifecycle; the OutboxInterceptor drains external events to the outbox on SaveChanges.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>Domain events raised by this aggregate since the last ClearDomainEvents call.</summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>Append a domain event to the collection.</summary>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>Remove all collected domain events (called after outbox drain).</summary>
    void ClearDomainEvents();
}
