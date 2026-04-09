using GSDT.Identity.Application.Queries.GetPermissions;

namespace GSDT.Identity.Application.Queries.GetRolePermissions;

/// <summary>Returns all permissions currently assigned to the specified role.</summary>
public sealed record GetRolePermissionsQuery(Guid RoleId) : IQuery<IReadOnlyList<PermissionDto>>;
