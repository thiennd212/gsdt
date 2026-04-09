namespace GSDT.Identity.Application.Queries.GetPermissions;

/// <summary>Returns all permissions, optionally filtered by module code or search term.</summary>
public sealed record GetPermissionsQuery(
    string? ModuleCode,
    string? Search) : IQuery<IReadOnlyList<PermissionDto>>;

/// <summary>Flat permission projection returned by permission queries.</summary>
public sealed record PermissionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string ModuleCode,
    string ResourceCode,
    string ActionCode);
