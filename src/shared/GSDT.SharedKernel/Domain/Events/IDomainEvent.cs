using MediatR;

namespace GSDT.SharedKernel.Domain.Events;

/// <summary>Base marker for all domain events. Implements MediatR INotification for in-process dispatch.</summary>
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
