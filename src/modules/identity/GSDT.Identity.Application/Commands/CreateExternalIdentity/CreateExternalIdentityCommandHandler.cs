using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.CreateExternalIdentity;

public sealed class CreateExternalIdentityCommandHandler(
    IExternalIdentityRepository repository,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<CreateExternalIdentityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateExternalIdentityCommand cmd, CancellationToken ct)
    {
        // Verify the user exists
        var user = await userManager.FindByIdAsync(cmd.UserId.ToString());
        if (user is null)
            return Result.Fail(new NotFoundError($"User {cmd.UserId} not found"));

        // Prevent duplicate provider link
        var existing = await repository.GetByUserAndProviderAsync(cmd.UserId, cmd.Provider, ct);
        if (existing is not null)
            return Result.Fail(new ConflictError(
                $"User {cmd.UserId} already has a linked {cmd.Provider} identity"));

        var entity = ExternalIdentity.Create(
            cmd.UserId, cmd.Provider, cmd.ExternalId,
            cmd.DisplayName, cmd.Email, cmd.ActorId);

        await repository.AddAsync(entity, ct);
        return Result.Ok(entity.Id);
    }
}
