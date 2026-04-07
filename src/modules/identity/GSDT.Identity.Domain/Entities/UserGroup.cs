namespace GSDT.Identity.Domain.Entities;

/// <summary>
/// Logical group of users that can be assigned roles in bulk (SEC_Group table).
/// Groups are tenant-scoped; Code is unique within a tenant.
/// </summary>
public class UserGroup
{
    public Guid Id { get; set; }

    /// <summary>Stable machine-readable code, e.g. "PHONG_TIEP_NHAN". Unique per tenant.</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? TenantId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<UserGroupMembership> Members { get; set; } = new List<UserGroupMembership>();
    public ICollection<GroupRoleAssignment> RoleAssignments { get; set; } = new List<GroupRoleAssignment>();
}
