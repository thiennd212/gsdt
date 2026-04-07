namespace GSDT.InvestmentProjects.Domain.Events;

/// <summary>Raised when an investment project is soft-deleted.</summary>
public sealed record ProjectDeletedEvent(Guid ProjectId, Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
