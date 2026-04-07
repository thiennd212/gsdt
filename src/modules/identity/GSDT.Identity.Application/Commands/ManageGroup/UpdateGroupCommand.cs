
namespace GSDT.Identity.Application.Commands.ManageGroup;

/// <summary>Update group name, description, and active status.</summary>
public sealed record UpdateGroupCommand(
    Guid GroupId,
    string Name,
    string? Description,
    bool? IsActive) : ICommand;

/// <summary>Remove a user from a group.</summary>
public sealed record RemoveUserFromGroupCommand(Guid GroupId, Guid UserId) : ICommand;

/// <summary>Remove a role assignment from a group.</summary>
public sealed record RemoveRoleFromGroupCommand(Guid GroupId, Guid RoleId) : ICommand;
