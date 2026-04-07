using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    ISender sender,
{
    public async Task<Result> Handle(ResetPasswordCommand cmd, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(cmd.UserId.ToString());
        if (user is null)
            return Result.Fail(new NotFoundError($"User {cmd.UserId} not found"));

        if (string.IsNullOrEmpty(user.Email))
            return Result.Fail("User has no email address configured.");

        // Generate secure reset token
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // Security (F-09): revoke all active tokens so old sessions cannot be reused
        await sender.Send(new RevokeTokenCommand(TokenId: null, UserId: cmd.UserId, ActorId: cmd.UserId), ct);

        // Security (F-25): publish event — Notifications module emails the token.
        // Token NEVER returned in API response.
        await events.PublishAsync(
            [new PasswordResetRequestedEvent(cmd.UserId, user.Email, user.FullName, token)], ct);

        return Result.Ok();
    }
}
