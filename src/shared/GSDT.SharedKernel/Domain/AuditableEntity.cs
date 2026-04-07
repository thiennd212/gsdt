using GSDT.SharedKernel.Domain.Events;

namespace GSDT.SharedKernel.Domain;

/// <summary>
/// Extends Entity with user audit trail, data classification (QĐ742), and
/// optional domain event collection for aggregate roots.
/// Subclasses that implement IAggregateRoot use AddDomainEvent / ClearDomainEvents;
/// the OutboxInterceptor drains IExternalDomainEvent entries to the outbox on SaveChanges.
/// </summary>
public abstract class AuditableEntity<TId> : Entity<TId>
{
    /// <summary>Backing store for domain events — populated via AddDomainEvent.</summary>
    protected readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>Read-only view of collected domain events — consumed by OutboxInterceptor and publishers.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid CreatedBy { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    /// <summary>Data classification per QĐ742: Public | Internal | Confidential | Secret | TopSecret.</summary>
    public ClassificationLevel ClassificationLevel { get; protected set; } = ClassificationLevel.Internal;

    public void SetAuditCreate(Guid userId) => CreatedBy = userId;

    public void SetAuditUpdate(Guid userId)
    {
        ModifiedBy = userId;
        MarkUpdated();
    }

    /// <summary>Appends a domain event — called from aggregate business methods.</summary>
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
