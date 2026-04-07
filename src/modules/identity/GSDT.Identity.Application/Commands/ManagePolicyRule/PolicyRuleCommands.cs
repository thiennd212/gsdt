
namespace GSDT.Identity.Application.Commands.ManagePolicyRule;

/// <summary>Create a new policy rule.</summary>
public sealed record CreatePolicyRuleCommand(
    string Code,
    string PermissionCode,
    string? ConditionExpression,
    string Effect,
    int Priority,
    bool LogOnDeny,
    string? Description,
    Guid? TenantId) : ICommand<Guid>;

/// <summary>Update an existing policy rule.</summary>
public sealed record UpdatePolicyRuleCommand(
    Guid Id,
    string PermissionCode,
    string? ConditionExpression,
    string Effect,
    int Priority,
    bool IsActive,
    bool LogOnDeny,
    string? Description) : ICommand;

/// <summary>Delete a policy rule by ID.</summary>
public sealed record DeletePolicyRuleCommand(Guid Id) : ICommand;
