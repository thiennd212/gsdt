namespace GSDT.InvestmentProjects.Domain.Events;

/// <summary>Raised when a new investment project (domestic or ODA) is created.</summary>
public sealed record ProjectCreatedEvent(Guid ProjectId, Guid TenantId, ProjectType Type) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
