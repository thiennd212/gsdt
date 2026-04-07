using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand cmd, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(cmd.UserId.ToString());
        if (user is null)
            return Result.Fail(new NotFoundError($"User {cmd.UserId} not found."));

        // Soft-delete: deactivate + lock account (no hard-delete for audit compliance)
        user.IsActive = false;
        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        await userManager.UpdateAsync(user);

        return Result.Ok();
    }
}
