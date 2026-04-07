using MediatR;

namespace GSDT.Infrastructure.Events;

/// <summary>
/// Dispatches internal domain events in-process via MediatR IPublisher.
/// External domain events are handled by MassTransit EF Outbox (Phase 02c).
/// </summary>
{
    {
        foreach (var domainEvent in events)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
