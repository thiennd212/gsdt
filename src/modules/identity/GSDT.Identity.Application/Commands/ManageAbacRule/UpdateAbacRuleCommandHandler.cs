using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.ManageAbacRule;

public sealed class UpdateAbacRuleCommandHandler(
    IAttributeRuleRepository repository,
    IAbacCacheInvalidator cacheInvalidator)
    : IRequestHandler<UpdateAbacRuleCommand, Result>
{
    public async Task<Result> Handle(UpdateAbacRuleCommand cmd, CancellationToken ct)
    {
        var rule = await repository.GetByIdAsync(cmd.RuleId, ct);
        if (rule is null)
            return Result.Fail(new NotFoundError($"ABAC rule {cmd.RuleId} not found."));

        rule.Resource = cmd.Resource;
        rule.Action = cmd.Action;
        rule.AttributeKey = cmd.AttributeKey;
        rule.AttributeValue = cmd.AttributeValue;
        rule.Effect = Enum.Parse<AttributeEffect>(cmd.Effect);
        rule.TenantId = cmd.TenantId;

        await repository.UpdateAsync(rule, ct);
        cacheInvalidator.InvalidateAll();

        return Result.Ok();
    }
}
