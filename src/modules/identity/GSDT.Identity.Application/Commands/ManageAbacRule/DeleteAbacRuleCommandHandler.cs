using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.ManageAbacRule;

public sealed class DeleteAbacRuleCommandHandler(
    IAttributeRuleRepository repository,
    IAbacCacheInvalidator cacheInvalidator)
    : IRequestHandler<DeleteAbacRuleCommand, Result>
{
    public async Task<Result> Handle(DeleteAbacRuleCommand cmd, CancellationToken ct)
    {
        await repository.DeleteAsync(cmd.RuleId, ct);

        // Invalidate all ABAC cache — we don't know which attribute value was deleted
        cacheInvalidator.InvalidateAll();

        return Result.Ok();
    }
}
