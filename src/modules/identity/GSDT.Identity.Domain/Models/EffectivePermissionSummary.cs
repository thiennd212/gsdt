
namespace GSDT.Identity.Domain.Models;

/// <summary>
/// Full snapshot of a user's effective permissions, derived from:
///   direct roles + group roles + active delegations → permission codes.
/// Consumers use PermissionCodes for fast Contains() checks.
/// </summary>
public sealed class EffectivePermissionSummary
{
    public Guid UserId { get; init; }

    /// <summary>Roles assigned directly to the user (AspNetUserRoles).</summary>
    public IReadOnlyList<RoleInfo> DirectRoles { get; init; } = [];

    /// <summary>Roles inherited via group membership (UserGroupMembership → GroupRoleAssignment).</summary>
    public IReadOnlyList<GroupRoleInfo> GroupRoles { get; init; } = [];

    /// <summary>Active delegations where this user is the delegate.</summary>
    public IReadOnlyList<DelegationInfo> ActiveDelegations { get; init; } = [];

    /// <summary>Union of all permission codes from all role sources.</summary>
    public IReadOnlySet<string> PermissionCodes { get; init; } = new HashSet<string>();

    /// <summary>Resolved data scope (merged from roles + overrides).</summary>
    public ResolvedDataScope DataScope { get; init; } = new();
}

/// <summary>A role directly assigned to the user.</summary>
public sealed record RoleInfo(
    Guid RoleId,
    string RoleCode,
    string RoleName,
    RoleType RoleType);

/// <summary>A role inherited through group membership.</summary>
public sealed record GroupRoleInfo(
    Guid GroupId,
    string GroupCode,
    Guid RoleId,
    string RoleCode);

/// <summary>An active delegation granting this user the delegator's roles.</summary>
public sealed record DelegationInfo(
    Guid DelegationId,
    Guid DelegatorId,
    string DelegatorName,
    DateTime ValidFrom,
    DateTime ValidTo);
