namespace GSDT.SharedKernel.Domain.Events;

/// <summary>
/// Raised by Identity module after admin requests a password reset.
/// Consumed by Notifications module to email the reset link.
/// Kept in SharedKernel so modules are not directly coupled.
/// </summary>
public sealed record PasswordResetRequestedEvent(
    Guid UserId,
    string Email,
    string FullName,
    string ResetToken) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
