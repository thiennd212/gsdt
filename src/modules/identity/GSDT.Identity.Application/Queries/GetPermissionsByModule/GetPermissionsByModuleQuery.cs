using GSDT.Identity.Application.Queries.GetPermissions;

namespace GSDT.Identity.Application.Queries.GetPermissionsByModule;

/// <summary>Returns all permissions grouped by module code.</summary>
public sealed record GetPermissionsByModuleQuery : IQuery<IReadOnlyList<ModulePermissionsDto>>;

/// <summary>Permission list for a single module.</summary>
public sealed record ModulePermissionsDto(
    string ModuleCode,
    IReadOnlyList<PermissionDto> Permissions);
