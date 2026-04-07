
namespace GSDT.Identity.Application.Commands.ManageDataScope;

/// <summary>Assign a data scope type to a role.</summary>
public sealed record CreateRoleDataScopeCommand(
    Guid RoleId,
    Guid DataScopeTypeId,
    string? ScopeField,
    string? ScopeValue,
    int Priority) : ICommand<Guid>;

/// <summary>Remove a data scope assignment from a role.</summary>
public sealed record DeleteRoleDataScopeCommand(Guid ScopeId) : ICommand;
