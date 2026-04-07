
namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for SodConflictRule.
/// Table: identity.SodConflictRules
/// Unique: (PermissionCodeA, PermissionCodeB, TenantId) — prevents duplicate pairs.
/// Note: pair (A,B) and (B,A) are treated as equivalent at the application layer.
/// </summary>
public sealed class SodConflictRuleConfiguration : IEntityTypeConfiguration<SodConflictRule>
{
    public void Configure(EntityTypeBuilder<SodConflictRule> builder)
    {
        builder.ToTable("SodConflictRules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.PermissionCodeA)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.PermissionCodeB)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.EnforcementLevel)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        // Unique per ordered pair + tenant
        builder.HasIndex(r => new { r.PermissionCodeA, r.PermissionCodeB, r.TenantId })
            .IsUnique();

        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => r.IsActive);
    }
}

/// <summary>
/// EF Core configuration for AppMenu.
/// Table: identity.AppMenus
/// Unique: (Code, TenantId) — stable machine-readable code per tenant.
/// Self-referencing FK: ParentId → Id (nullable, no cascade to avoid cycles).
/// </summary>
public sealed class AppMenuConfiguration : IEntityTypeConfiguration<AppMenu>
{
    public void Configure(EntityTypeBuilder<AppMenu> builder)
    {
        builder.ToTable("AppMenus");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Icon)
            .HasMaxLength(100);

        builder.Property(m => m.Route)
            .HasMaxLength(500);

        // Code unique per tenant
        builder.HasIndex(m => new { m.Code, m.TenantId }).IsUnique();
        builder.HasIndex(m => m.TenantId);
        builder.HasIndex(m => m.ParentId);

        // Self-referencing FK — restrict delete to avoid cascade cycles
        builder.HasOne<AppMenu>()
            .WithMany()
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(m => m.RolePermissions)
            .WithOne(rp => rp.Menu)
            .HasForeignKey(rp => rp.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for MenuRolePermission.
/// Table: identity.MenuRolePermissions
/// Composite PK: (MenuId, PermissionCode).
/// </summary>
public sealed class MenuRolePermissionConfiguration : IEntityTypeConfiguration<MenuRolePermission>
{
    public void Configure(EntityTypeBuilder<MenuRolePermission> builder)
    {
        builder.ToTable("MenuRolePermissions");

        // Composite primary key
        builder.HasKey(rp => new { rp.MenuId, rp.PermissionCode });

        builder.Property(rp => rp.PermissionCode)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(rp => rp.PermissionCode);
    }
}
