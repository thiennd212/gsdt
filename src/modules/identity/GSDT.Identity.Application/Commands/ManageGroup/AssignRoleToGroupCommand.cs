
namespace GSDT.Identity.Application.Commands.ManageGroup;

/// <summary>Assign a role to a group. Idempotent — no error if assignment already exists.</summary>
public sealed record AssignRoleToGroupCommand(
    Guid GroupId,
    Guid RoleId,
    Guid AssignedBy) : ICommand;
