namespace GSDT.Organization.DTOs;

/// <summary>Flat DTO for a single org unit — clients build tree from ParentId.</summary>
public sealed record OrgUnitDto(
    Guid Id,
    Guid? ParentId,
    string Name,
    string NameEn,
    string Code,
    int Level,
    bool IsActive,
    Guid TenantId,
    Guid? SuccessorId);

/// <summary>Staff assignment DTO.</summary>
public sealed record UserOrgUnitAssignmentDto(
    Guid Id,
    Guid UserId,
    Guid OrgUnitId,
    string RoleInOrg,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive);

/// <summary>Position history DTO.</summary>
public sealed record StaffPositionHistoryDto(
    Guid Id,
    Guid UserId,
    Guid OrgUnitId,
    string PositionTitle,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate);

/// <summary>Request body for creating an org unit.</summary>
public sealed record CreateOrgUnitRequest(
    string Name,
    string NameEn,
    string Code,
    Guid? ParentId);

/// <summary>Request body for updating an org unit.</summary>
public sealed record UpdateOrgUnitRequest(
    string Name,
    string NameEn);

/// <summary>Request body for assigning a user to an org unit.</summary>
public sealed record AssignStaffRequest(
    Guid OrgUnitId,
    string RoleInOrg,
    string PositionTitle);

/// <summary>Request body for transferring a user to a new org unit.</summary>
public sealed record TransferStaffRequest(
    Guid ToOrgUnitId,
    string RoleInOrg,
    string PositionTitle);
