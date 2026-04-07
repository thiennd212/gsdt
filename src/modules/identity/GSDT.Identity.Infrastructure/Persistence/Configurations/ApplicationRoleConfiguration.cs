
namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(r => r.Code).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);

        // RoleType stored as int column
        builder.Property(r => r.RoleType).HasConversion<int>();

        builder.HasIndex(r => r.TenantId);

        // Code must be unique within a tenant (null TenantId = system-wide role)
        builder.HasIndex(r => new { r.Code, r.TenantId }).IsUnique();

        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.GroupRoles)
            .WithOne(gra => gra.Role)
            .HasForeignKey(gra => gra.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
