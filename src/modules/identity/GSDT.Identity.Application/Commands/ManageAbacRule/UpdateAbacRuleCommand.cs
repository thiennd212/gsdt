
namespace GSDT.Identity.Application.Commands.ManageAbacRule;

/// <summary>Update an existing ABAC attribute rule.</summary>
public sealed record UpdateAbacRuleCommand(
    Guid RuleId,
    string Resource,
    string Action,
    string AttributeKey,
    string AttributeValue,
    string Effect,
    Guid? TenantId) : ICommand;
