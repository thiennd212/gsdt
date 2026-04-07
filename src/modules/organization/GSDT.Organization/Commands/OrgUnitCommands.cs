
namespace GSDT.Organization.Commands;

/// <summary>Create a new org unit under an optional parent.</summary>
public sealed record CreateOrgUnitCommand(
    Guid TenantId,
    string Name,
    string NameEn,
    string Code,
    Guid? ParentId) : ICommand<OrgUnitDto>;

/// <summary>Update name fields of an existing org unit.</summary>
public sealed record UpdateOrgUnitCommand(
    Guid Id,
    Guid TenantId,
    string Name,
    string NameEn) : ICommand<OrgUnitDto>;

/// <summary>Soft-deactivate an org unit (blocked if active children exist).</summary>
public sealed record DeleteOrgUnitCommand(
    Guid Id,
    Guid TenantId,
    Guid? SuccessorId) : ICommand;

/// <summary>Assign a user to an org unit with a role and position title.</summary>
public sealed record AssignStaffCommand(
    Guid TenantId,
    Guid UserId,
    Guid OrgUnitId,
    string RoleInOrg,
    string PositionTitle) : ICommand<UserOrgUnitAssignmentDto>;

/// <summary>Transfer a user from current org unit to a new one — closes old assignment, opens new.</summary>
public sealed record TransferStaffCommand(
    Guid TenantId,
    Guid UserId,
    Guid ToOrgUnitId,
    string RoleInOrg,
    string PositionTitle) : ICommand<UserOrgUnitAssignmentDto>;
