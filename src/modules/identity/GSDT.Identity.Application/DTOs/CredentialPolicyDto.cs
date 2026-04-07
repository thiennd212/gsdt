namespace GSDT.Identity.Application.DTOs;

/// <summary>Read DTO for CredentialPolicy.</summary>
public sealed record CredentialPolicyDto(
    Guid Id,
    string Name,
    Guid TenantId,
    int MinLength,
    int MaxLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireDigit,
    bool RequireSpecialChar,
    int RotationDays,
    int MaxFailedAttempts,
    int LockoutMinutes,
    int PasswordHistoryCount,
    bool IsDefault);
