namespace GSDT.SharedKernel.Domain.Events;

/// <summary>Dispatches collected domain events after SaveChanges completes.</summary>
public interface IDomainEventPublisher
{
    Task PublishEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
