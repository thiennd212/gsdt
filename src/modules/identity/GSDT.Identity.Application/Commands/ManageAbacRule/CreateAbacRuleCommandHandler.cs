using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.ManageAbacRule;

public sealed class CreateAbacRuleCommandHandler(
    IAttributeRuleRepository repository,
    IAbacCacheInvalidator cacheInvalidator)
    : IRequestHandler<CreateAbacRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAbacRuleCommand cmd, CancellationToken ct)
    {
        if (!Enum.TryParse<AttributeEffect>(cmd.Effect, true, out var effect))
            return Result.Fail(new ValidationError($"Invalid effect: {cmd.Effect}. Use 'Allow' or 'Deny'."));

        var rule = AttributeRule.Create(
            cmd.Resource, cmd.Action, cmd.AttributeKey, cmd.AttributeValue, effect, cmd.TenantId);

        await repository.AddAsync(rule, ct);

        // Invalidate ABAC cache so new rules take effect immediately
        cacheInvalidator.InvalidateByAttributeValue(cmd.AttributeValue);

        return Result.Ok(rule.Id);
    }
}
