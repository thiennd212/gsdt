using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.UpdateExternalIdentity;

public sealed class UpdateExternalIdentityCommandHandler(
    IExternalIdentityRepository repository)
    : IRequestHandler<UpdateExternalIdentityCommand, Result>
{
    public async Task<Result> Handle(UpdateExternalIdentityCommand cmd, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(cmd.Id, ct);
        if (entity is null)
            return Result.Fail(new NotFoundError($"ExternalIdentity {cmd.Id} not found"));

        entity.UpdateSync(cmd.DisplayName, cmd.Email, cmd.Metadata, cmd.ActorId);
        await repository.UpdateAsync(entity, ct);
        return Result.Ok();
    }
}
