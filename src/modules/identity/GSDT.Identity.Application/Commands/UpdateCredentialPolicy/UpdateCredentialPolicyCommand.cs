
namespace GSDT.Identity.Application.Commands.UpdateCredentialPolicy;

public sealed record UpdateCredentialPolicyCommand(
    Guid Id,
    string Name,
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
    Guid ActorId) : ICommand;
