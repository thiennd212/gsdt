using System.Text.Json;

namespace GSDT.Identity.Domain.Entities;

/// <summary>
/// Delegation lifecycle states.
/// </summary>
public enum DelegationStatus
{
    PendingApproval = 1,
    Active = 2,
    Expired = 3,
    Revoked = 4
}

/// <summary>
/// Full role delegation: User A delegates all (or a scoped subset of) roles to User B for a time window.
/// DelegationClaimsTransformer injects delegator's roles into delegate's JWT claims.
///
/// Enhancement (Phase E):
///   - DelegatedRoleIds: optional JSON array of Guid — restrict which roles are delegated.
///     Null = inject all delegator roles (backward-compat).
///   - ScopeJson: optional JSON scope override.
///   - Status: lifecycle enum (PendingApproval → Active → Expired/Revoked).
///   - RequiresApproval: if true, delegation stays PendingApproval until Admin approves.
///   - ApprovedBy / ApprovedAtUtc: approval audit trail.
/// </summary>
public class UserDelegation
{
    public Guid Id { get; set; }
    public Guid DelegatorId { get; set; }
    public Guid DelegateId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string? Reason { get; set; }

    // --- Legacy revoke flag (kept for backward compat; Status = Revoked is the canonical signal) ---
    public bool IsRevoked { get; set; }
    public Guid? RevokedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // --- Phase E additions ---

    /// <summary>
    /// JSON array of role Guid strings to restrict delegation scope.
    /// Null = delegate all delegator roles (backward-compat behavior).
    /// </summary>
    public string? DelegatedRoleIds { get; set; }

    /// <summary>JSON-serialized scope override object (reserved for future fine-grained scope).</summary>
    public string? ScopeJson { get; set; }

    /// <summary>Delegation lifecycle status.</summary>
    public DelegationStatus Status { get; set; } = DelegationStatus.Active;

    /// <summary>When true, delegation waits for Admin approval before becoming Active.</summary>
    public bool RequiresApproval { get; set; }

    /// <summary>ID of Admin who approved the delegation.</summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>UTC timestamp when the delegation was approved.</summary>
    public DateTime? ApprovedAtUtc { get; set; }

    // --- Navigation ---
    public ApplicationUser Delegator { get; set; } = null!;
    public ApplicationUser Delegate { get; set; } = null!;

    // --- Domain helpers ---

    /// <summary>
    /// Returns the list of role IDs scoped to this delegation,
    /// or null if all delegator roles should be injected (backward compat).
    /// </summary>
    public IReadOnlyList<Guid>? GetDelegatedRoleIds()
    {
        if (string.IsNullOrWhiteSpace(DelegatedRoleIds))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(DelegatedRoleIds);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns true when the delegation is currently effective:
    /// Status == Active, not revoked, and within the valid time window.
    /// </summary>
    public bool IsEffective(DateTime now) =>
        Status == DelegationStatus.Active
        && !IsRevoked
        && ValidFrom <= now
        && ValidTo >= now;
}
