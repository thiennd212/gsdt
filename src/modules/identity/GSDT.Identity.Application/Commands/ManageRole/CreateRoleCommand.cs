using GSDT.Identity.Application.Queries.GetRoleById;

namespace GSDT.Identity.Application.Commands.ManageRole;

/// <summary>Creates a new business role. Code must be unique across all tenants.</summary>
public sealed record CreateRoleCommand(
    string Code,
    string Name,
    string? Description) : ICommand<RoleDetailDto>;
