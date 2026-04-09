namespace GSDT.Identity.Application.Queries.GetRoleById;

/// <summary>Full role detail including assigned permissions — used by GetRoleByIdQuery and Create/Update responses.</summary>
public sealed record RoleDetailDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string RoleType,
    bool IsActive,
    IReadOnlyList<RolePermissionDto> Permissions);

/// <summary>Slim permission view attached to a role detail response.</summary>
public sealed record RolePermissionDto(
    Guid PermissionId,
    string Code,
    string Name);
