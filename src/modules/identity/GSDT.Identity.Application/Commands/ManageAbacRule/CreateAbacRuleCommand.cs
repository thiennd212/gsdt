
namespace GSDT.Identity.Application.Commands.ManageAbacRule;

/// <summary>Create a new ABAC attribute rule for department/classification access control.</summary>
public sealed record CreateAbacRuleCommand(
    string Resource,
    string Action,
    string AttributeKey,
    string AttributeValue,
    string Effect,
    Guid? TenantId) : ICommand<Guid>;
