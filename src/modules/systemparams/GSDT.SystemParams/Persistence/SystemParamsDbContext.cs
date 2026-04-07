
namespace GSDT.SystemParams.Persistence;

/// <summary>EF Core DbContext for config schema — SystemParameters + SystemAnnouncements.</summary>
public class SystemParamsDbContext(DbContextOptions<SystemParamsDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    protected override string SchemaName => "config";

    public DbSet<SystemParameter> SystemParameters => Set<SystemParameter>();
    public DbSet<SystemAnnouncement> SystemAnnouncements => Set<SystemAnnouncement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SystemParameter>(e =>
        {
            e.ToTable("SystemParameters");
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(200).IsRequired();
            e.Property(x => x.Value).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.TenantId).HasMaxLength(100);
            e.Property(x => x.DataType).HasConversion<string>().HasMaxLength(20);
            // Unique per (Key, TenantId) — SQL Server handles nullable correctly
            e.HasIndex(x => new { x.Key, x.TenantId }).IsUnique();
        });

        modelBuilder.Entity<SystemAnnouncement>(e =>
        {
            e.ToTable("SystemAnnouncements");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.Property(x => x.Content).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TargetRoles).HasColumnType("nvarchar(max)");
            e.HasIndex(x => new { x.IsActive, x.StartAt, x.EndAt });
        });
    }
}
