using MediatR;

namespace GSDT.Infrastructure.Events;

/// <summary>
/// Dispatches internal domain events in-process via MediatR IPublisher.
/// External domain events are handled by MassTransit EF Outbox (Phase 02c).
/// </summary>
public sealed class MediatRDomainEventPublisher(IPublisher publisher) : IDomainEventPublisher
{
    public async Task PublishEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            await publisher.Publish(domainEvent, ct);
        }
    }
}
