using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand cmd, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(cmd.UserId.ToString());
        if (user is null)
            return Result.Fail(new NotFoundError($"User {cmd.UserId} not found."));

        user.FullName = cmd.FullName;
        user.DepartmentCode = cmd.DepartmentCode;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Fail(result.Errors.Select(e => new ValidationError(e.Description)));

        return Result.Ok();
    }
}
