
namespace GSDT.Audit.Domain.Entities;

/// <summary>Records every authentication attempt — for suspicious activity detection.</summary>
public sealed class LoginAttempt
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid? UserId { get; private set; }
    [DataClassification(DataClassificationLevel.Confidential)]
    public string Email { get; private set; } = string.Empty;
    [DataClassification(DataClassificationLevel.Confidential)]
    public string IpAddress { get; private set; } = string.Empty;
    [DataClassification(DataClassificationLevel.Confidential)]
    public string? UserAgent { get; private set; }
    public bool Success { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset AttemptedAt { get; private set; } = DateTimeOffset.UtcNow;

    private LoginAttempt() { }

    public static LoginAttempt Record(
        Guid? userId, string email, string ipAddress, string? userAgent,
        bool success, string? failureReason = null) =>
        new()
        {
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            FailureReason = failureReason
        };
}
