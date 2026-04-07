using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.LockUser;

public sealed class LockUserCommandHandler : IRequestHandler<LockUserCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    {
        _userManager = userManager;
        _events = events;
    }

    public async Task<Result> Handle(LockUserCommand cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(cmd.UserId.ToString());
        if (user is null)
            return Result.Fail(new NotFoundError($"User {cmd.UserId} not found"));

        user.IsActive = !cmd.Lock;

        // ASP.NET Core Identity lockout — also set LockoutEnd for Lock=true
        await _userManager.SetLockoutEnabledAsync(user, cmd.Lock);
        if (cmd.Lock)
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        else
            await _userManager.SetLockoutEndDateAsync(user, null);

        await _userManager.UpdateAsync(user);

        await _events.PublishAsync([new UserLockedEvent(cmd.UserId, cmd.Lock, cmd.ActorId)], ct);
        return Result.Ok();
    }
}
