
namespace GSDT.Identity.Application.Commands.SyncUserRoles;

/// <summary>Replace all roles for a user — removes roles not in the list, adds new ones.</summary>
public sealed record SyncUserRolesCommand(
    Guid UserId,
    IReadOnlyList<string> Roles,
    Guid UpdatedBy) : ICommand;
