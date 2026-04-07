
namespace GSDT.Identity.Application.Commands.ChangePassword;

/// <summary>Self-service password change — requires current password verification.</summary>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand;
