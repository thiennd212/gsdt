using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.DeleteExternalIdentity;

public sealed class DeleteExternalIdentityCommandHandler(
    IExternalIdentityRepository repository)
    : IRequestHandler<DeleteExternalIdentityCommand, Result>
{
    public async Task<Result> Handle(DeleteExternalIdentityCommand cmd, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(cmd.Id, ct);
        if (entity is null)
            return Result.Fail(new NotFoundError($"ExternalIdentity {cmd.Id} not found"));

        await repository.DeleteAsync(entity, ct);
        return Result.Ok();
    }
}
