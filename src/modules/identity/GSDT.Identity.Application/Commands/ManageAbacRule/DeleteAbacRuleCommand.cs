
namespace GSDT.Identity.Application.Commands.ManageAbacRule;

/// <summary>Delete an ABAC attribute rule by ID.</summary>
public sealed record DeleteAbacRuleCommand(Guid RuleId) : ICommand;
