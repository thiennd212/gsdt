
namespace GSDT.Organization.Infrastructure.Persistence;

/// <summary>EF Core DbContext for organization module — schema "organization".</summary>
public class OrgDbContext(DbContextOptions<OrgDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    protected override string SchemaName => "organization";

    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();
    public DbSet<UserOrgUnitAssignment> Assignments => Set<UserOrgUnitAssignment>();
    public DbSet<StaffPositionHistory> PositionHistories => Set<StaffPositionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrgUnit>(e =>
        {
            e.ToTable("OrgUnits");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Level).IsRequired();
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.HasIndex(x => new { x.TenantId, x.ParentId });
            e.HasOne<OrgUnit>()
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<UserOrgUnitAssignment>(e =>
        {
            e.ToTable("UserOrgUnitAssignments");
            e.HasKey(x => x.Id);
            e.Property(x => x.RoleInOrg).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive });
            e.HasIndex(x => new { x.TenantId, x.OrgUnitId });
        });

        modelBuilder.Entity<StaffPositionHistory>(e =>
        {
            e.ToTable("StaffPositionHistories");
            e.HasKey(x => x.Id);
            e.Property(x => x.PositionTitle).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.TenantId, x.UserId });
            e.HasIndex(x => new { x.TenantId, x.OrgUnitId });
        });
    }
}
