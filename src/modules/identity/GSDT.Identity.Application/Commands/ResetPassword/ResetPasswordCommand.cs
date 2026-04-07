using FluentResults;

namespace GSDT.Identity.Application.Commands.ResetPassword;

/// <summary>Admin-initiated password reset — generates token and emails it to the user (F-25).</summary>
public sealed record ResetPasswordCommand(Guid UserId, Guid ActorId) : ICommand;
