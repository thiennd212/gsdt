using GSDT.Identity.Application.Queries.GetRoleById;

namespace GSDT.Identity.Application.Commands.ManageRole;

/// <summary>
/// Updates Name and Description of an existing role.
/// For System roles, only Description can be changed — Name is immutable.
/// </summary>
public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description) : ICommand<RoleDetailDto>;
