
namespace GSDT.Identity.Application.Commands.CreateCredentialPolicy;

public sealed record CreateCredentialPolicyCommand(
    string Name,
    Guid TenantId,
    int MinLength,
    int MaxLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireDigit,
    bool RequireSpecialChar,
    int RotationDays,
    int MaxFailedAttempts,
    int LockoutMinutes,
    int PasswordHistoryCount,
    bool IsDefault,
    Guid ActorId) : ICommand<Guid>;
