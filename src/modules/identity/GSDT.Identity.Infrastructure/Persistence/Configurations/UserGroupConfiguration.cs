
namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("UserGroups");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Code).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Name).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(500);

        // Code unique per tenant
        builder.HasIndex(g => new { g.Code, g.TenantId }).IsUnique();
        builder.HasIndex(g => g.TenantId);

        builder.HasMany(g => g.Members)
            .WithOne(m => m.Group)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.RoleAssignments)
            .WithOne(gra => gra.Group)
            .HasForeignKey(gra => gra.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class UserGroupMembershipConfiguration : IEntityTypeConfiguration<UserGroupMembership>
{
    public void Configure(EntityTypeBuilder<UserGroupMembership> builder)
    {
        builder.ToTable("UserGroupMemberships");

        builder.HasKey(m => m.Id);

        // One membership record per user per group
        builder.HasIndex(m => new { m.UserId, m.GroupId }).IsUnique();
        builder.HasIndex(m => m.GroupId);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class GroupRoleAssignmentConfiguration : IEntityTypeConfiguration<GroupRoleAssignment>
{
    public void Configure(EntityTypeBuilder<GroupRoleAssignment> builder)
    {
        builder.ToTable("GroupRoleAssignments");

        builder.HasKey(gra => gra.Id);

        // One assignment per group per role
        builder.HasIndex(gra => new { gra.GroupId, gra.RoleId }).IsUnique();
        builder.HasIndex(gra => gra.RoleId);
    }
}
