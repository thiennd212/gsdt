
namespace GSDT.Identity.Application.Queries.GetRoles;

/// <summary>Returns all roles from the database with name, description, and permission count.</summary>
public sealed record GetRolesQuery : IQuery<IReadOnlyList<RoleDefinitionDto>>;

public sealed record RoleDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string RoleType,
    bool IsActive,
    int PermissionCount = 0);
