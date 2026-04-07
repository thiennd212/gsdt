namespace GSDT.Identity.Domain.Entities;

/// <summary>
/// Segregation of Duties conflict rule — defines pairs of permissions that
/// must not coexist on the same user. Enforcement level controls the outcome
/// when a violation is detected during role assignment.
/// </summary>
public class SodConflictRule
{
    public Guid Id { get; set; }

    /// <summary>First permission code in the conflicting pair.</summary>
    public string PermissionCodeA { get; set; } = string.Empty;

    /// <summary>Second permission code in the conflicting pair.</summary>
    public string PermissionCodeB { get; set; } = string.Empty;

    /// <summary>How the system responds when both permissions are held by one user.</summary>
    public SodEnforcementLevel EnforcementLevel { get; set; } = SodEnforcementLevel.Warn;

    /// <summary>Human-readable explanation of why these permissions conflict.</summary>
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? TenantId { get; set; }
}

/// <summary>Enforcement action when a SoD conflict is detected.</summary>
public enum SodEnforcementLevel
{
    /// <summary>Log warning — role assignment proceeds.</summary>
    Warn = 1,

    /// <summary>Hard block — role assignment is rejected.</summary>
    Block = 2,

    /// <summary>Requires an approval workflow before the role can be granted.</summary>
    RequireApproval = 3
}
