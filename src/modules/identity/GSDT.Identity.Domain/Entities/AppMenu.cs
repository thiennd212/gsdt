namespace GSDT.Identity.Domain.Entities;

/// <summary>
/// Application menu item — supports hierarchical structure via ParentId self-reference.
/// Access is controlled by MenuRolePermission entries: a user sees a menu item
/// if they hold at least one of its required permission codes.
/// </summary>
public class AppMenu
{
    public Guid Id { get; set; }

    /// <summary>Parent menu item ID — null means top-level.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Stable machine-readable code, unique per tenant.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Display title shown in the navigation UI.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional icon identifier (e.g., Ant Design icon name).</summary>
    public string? Icon { get; set; }

    /// <summary>Frontend route path (e.g., "/admin/users").</summary>
    public string? Route { get; set; }

    /// <summary>Display order within the same parent level (ascending).</summary>
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<MenuRolePermission> RolePermissions { get; set; } = [];
}
