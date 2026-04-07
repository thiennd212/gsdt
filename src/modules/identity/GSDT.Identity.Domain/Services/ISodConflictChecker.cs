namespace GSDT.Identity.Domain.Services;

/// <summary>
/// Checks for Segregation of Duties violations when assigning a role to a user.
/// Compares the user's current effective permissions against the permissions
/// carried by the role being assigned.
/// </summary>
public interface ISodConflictChecker
{
    /// <summary>
    /// Returns all active SoD conflicts that would arise if <paramref name="roleToAssign"/>
    /// were granted to <paramref name="userId"/>.
    /// Empty list means no violations.
    /// </summary>
    Task<IReadOnlyList<SodViolation>> CheckAsync(
        Guid userId,
        Guid roleToAssign,
        CancellationToken ct = default);
}

/// <summary>
/// Describes a single SoD conflict detected between two permission codes.
/// </summary>
public sealed record SodViolation(
    string PermissionCodeA,
    string PermissionCodeB,
    string Level,
    string? Description);
