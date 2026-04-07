namespace GSDT.Organization.Application.DTOs;

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
    Guid? SuccessorId,
    int ChildCount = 0,
    int StaffCount = 0);

/// <summary>Staff assignment DTO — includes user name/email for display.</summary>
public sealed record UserOrgUnitAssignmentDto(
    Guid Id,
    Guid UserId,
    Guid OrgUnitId,
    string RoleInOrg,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    string? FullName = null,
    string? Email = null);

/// <summary>Position history DTO.</summary>
public sealed record StaffPositionHistoryDto(
    Guid Id,
    Guid UserId,
    Guid OrgUnitId,
    string PositionTitle,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate);
