
namespace GSDT.Identity.Application.Queries.ListAbacRules;

public sealed record ListAbacRulesQuery(Guid? TenantId) : IQuery<IReadOnlyList<AbacRuleDto>>;

public sealed record AbacRuleDto(
    Guid Id, string Resource, string Action,
    string AttributeKey, string AttributeValue,
    string Effect, Guid? TenantId);
