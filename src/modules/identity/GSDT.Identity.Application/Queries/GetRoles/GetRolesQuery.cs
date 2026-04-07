
namespace GSDT.Identity.Application.Queries.GetRoles;

/// <summary>Returns the static list of role definitions (RBAC catalogue — no DB required).</summary>
public sealed record GetRolesQuery : IQuery<IReadOnlyList<RoleDefinitionDto>>;

public sealed record RoleDefinitionDto(
    Guid Id,
    string Name,
    string? Description);
