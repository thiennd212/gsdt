
namespace GSDT.Identity.Application.DTOs;

/// <summary>User read DTO — PII masked for non-Admin callers by controller layer.</summary>
public sealed record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? DepartmentCode,
    ClassificationLevel ClearanceLevel,
    bool IsActive,
    Guid? TenantId,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAt,
    DateTime? PasswordExpiresAt,
    IReadOnlyList<string> Roles);

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? TenantId);

/// <summary>Result DTO for bulk user import operation.</summary>
public sealed record BulkImportResultDto(
    int SuccessCount,
    int FailedCount,
    IReadOnlyList<BulkImportRowError> Errors);

public sealed record BulkImportRowError(int RowNumber, string Email, string Reason);

/// <summary>Active session info for session management admin API.</summary>
public sealed record ActiveSessionDto(
    string TokenId,
    Guid UserId,
    string? UserEmail,
    DateTime IssuedAt,
    DateTime ExpiresAt,
    string? IpAddress,
    string? ClientId);

/// <summary>Access review pending item.</summary>
public sealed record AccessReviewDto(
    Guid Id,
    Guid UserId,
    string? UserEmail,
    Guid RoleId,
    string? RoleName,
    DateTime NextReviewDue,
    DateTime CreatedAtUtc);
