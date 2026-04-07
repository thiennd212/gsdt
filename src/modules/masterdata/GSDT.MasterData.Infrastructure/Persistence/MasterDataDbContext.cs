
namespace GSDT.MasterData.Infrastructure.Persistence;

/// <summary>EF Core DbContext for masterdata schema — Province/District/Ward/CaseType/JobTitle/AdministrativeUnit + Dictionary entities.</summary>
public class MasterDataDbContext(DbContextOptions<MasterDataDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    protected override string SchemaName => "masterdata";

    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<AdministrativeUnit> AdministrativeUnits => Set<AdministrativeUnit>();
    public DbSet<CaseType> CaseTypes => Set<CaseType>();
    public DbSet<JobTitle> JobTitles => Set<JobTitle>();

    public DbSet<Dictionary> Dictionaries => Set<Dictionary>();
    public DbSet<DictionaryItem> DictionaryItems => Set<DictionaryItem>();
    public DbSet<ExternalMapping> ExternalMappings => Set<ExternalMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Province>(e =>
        {
            e.ToTable("Provinces");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(10).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<District>(e =>
        {
            e.ToTable("Districts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(10).IsRequired();
            e.Property(x => x.ProvinceCode).HasMaxLength(10).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.ProvinceCode);
        });

        modelBuilder.Entity<Ward>(e =>
        {
            e.ToTable("Wards");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(10).IsRequired();
            e.Property(x => x.DistrictCode).HasMaxLength(10).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.DistrictCode);
        });

        modelBuilder.Entity<AdministrativeUnit>(e =>
        {
            e.ToTable("AdministrativeUnits");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(300).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(300).IsRequired();
            e.Property(x => x.ParentCode).HasMaxLength(20);
            e.Property(x => x.SuccessorCode).HasMaxLength(20);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => new { x.Level, x.ParentCode });
        });

        modelBuilder.Entity<CaseType>(e =>
        {
            e.ToTable("CaseTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
            e.Property(x => x.TenantId).HasMaxLength(100);
            e.HasIndex(x => new { x.Code, x.TenantId }).IsUnique();
        });

        modelBuilder.Entity<JobTitle>(e =>
        {
            e.ToTable("JobTitles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
            e.Property(x => x.TenantId).HasMaxLength(100);
            e.HasIndex(x => new { x.Code, x.TenantId }).IsUnique();
        });

        ConfigureDictionaryEntities(modelBuilder);
    }

    private static void ConfigureDictionaryEntities(ModelBuilder mb)
    {
        mb.Entity<Dictionary>(e =>
        {
            e.ToTable("Dictionaries");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasIndex(x => new { x.Code, x.TenantId }).IsUnique();
            e.HasIndex(x => x.TenantId);
        });

        mb.Entity<DictionaryItem>(e =>
        {
            e.ToTable("DictionaryItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(200).IsRequired();
            e.Property(x => x.Metadata).HasColumnType("nvarchar(max)");
            e.HasIndex(x => new { x.DictionaryId, x.Code }).IsUnique();
            e.HasIndex(x => new { x.DictionaryId, x.ParentId });
            e.HasOne(x => x.Parent).WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Dictionary).WithMany(x => x.Items)
                .HasForeignKey(x => x.DictionaryId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<ExternalMapping>(e =>
        {
            e.ToTable("ExternalMappings");
            e.HasKey(x => x.Id);
            e.Property(x => x.InternalCode).HasMaxLength(100).IsRequired();
            e.Property(x => x.ExternalSystem).HasMaxLength(100).IsRequired();
            e.Property(x => x.ExternalCode).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.ExternalSystem, x.ExternalCode, x.TenantId });
            e.HasIndex(x => new { x.InternalCode, x.ExternalSystem, x.TenantId });
            e.HasOne(x => x.Dictionary).WithMany()
                .HasForeignKey(x => x.DictionaryId).OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }
}
