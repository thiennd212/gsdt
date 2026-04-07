using GSDT.SharedKernel.Domain.Events;

namespace GSDT.ModuleName.Domain.Events;

public sealed record ModuleNameCreatedEvent(
    Guid EntityId,
    Guid TenantId,
    Guid CreatedBy) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
