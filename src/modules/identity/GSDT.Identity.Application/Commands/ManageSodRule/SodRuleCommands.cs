
namespace GSDT.Identity.Application.Commands.ManageSodRule;

/// <summary>Create a new SoD conflict rule.</summary>
public sealed record CreateSodRuleCommand(
    string PermissionCodeA,
    string PermissionCodeB,
    string EnforcementLevel,
    string? Description,
    Guid? TenantId) : ICommand<Guid>;

/// <summary>Update an existing SoD conflict rule.</summary>
public sealed record UpdateSodRuleCommand(
    Guid Id,
    string PermissionCodeA,
    string PermissionCodeB,
    string EnforcementLevel,
    string? Description,
    bool IsActive) : ICommand;

/// <summary>Delete a SoD conflict rule by ID.</summary>
public sealed record DeleteSodRuleCommand(Guid Id) : ICommand;
