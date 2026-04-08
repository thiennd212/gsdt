
using GSDT.MasterData.Entities.Catalogs;
using GSDT.MasterData.Enums;

namespace GSDT.MasterData.Persistence;

/// <summary>
/// EF Core DbContext for masterdata schema.
/// Covers: Province/District/Ward/AdministrativeUnit/CaseType/JobTitle,
/// Dictionary/DictionaryItem/ExternalMapping,
/// 14 seed catalogs, 10 tenant-scoped dynamic catalogs, ContractorSelectionPlan.
/// </summary>
public class MasterDataDbContext(DbContextOptions<MasterDataDbContext> options, ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    protected override string SchemaName => "masterdata";

    // ── Geographic / admin ────────────────────────────────────────────────────
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<AdministrativeUnit> AdministrativeUnits => Set<AdministrativeUnit>();
    public DbSet<CaseType> CaseTypes => Set<CaseType>();
    public DbSet<JobTitle> JobTitles => Set<JobTitle>();

    // ── Dictionary ────────────────────────────────────────────────────────────
    public DbSet<Dictionary> Dictionaries => Set<Dictionary>();
    public DbSet<DictionaryItem> DictionaryItems => Set<DictionaryItem>();
    public DbSet<ExternalMapping> ExternalMappings => Set<ExternalMapping>();

    // ── Seed catalogs (14) ────────────────────────────────────────────────────
    public DbSet<IndustrySector> IndustrySectors => Set<IndustrySector>();
    public DbSet<ProjectGroup> ProjectGroups => Set<ProjectGroup>();
    public DbSet<DomesticProjectStatus> DomesticProjectStatuses => Set<DomesticProjectStatus>();
    public DbSet<OdaProjectStatus> OdaProjectStatuses => Set<OdaProjectStatus>();
    public DbSet<AdjustmentContent> AdjustmentContents => Set<AdjustmentContent>();
    public DbSet<BidSelectionForm> BidSelectionForms => Set<BidSelectionForm>();
    public DbSet<BidSelectionMethod> BidSelectionMethods => Set<BidSelectionMethod>();
    public DbSet<ContractForm> ContractForms => Set<ContractForm>();
    public DbSet<BidSectorType> BidSectorTypes => Set<BidSectorType>();
    public DbSet<EvaluationType> EvaluationTypes => Set<EvaluationType>();
    public DbSet<AuditConclusionType> AuditConclusionTypes => Set<AuditConclusionType>();
    public DbSet<ViolationType> ViolationTypes => Set<ViolationType>();
    public DbSet<ViolationAction> ViolationActions => Set<ViolationAction>();
    public DbSet<OdaProjectType> OdaProjectTypes => Set<OdaProjectType>();

    // ── Dynamic tenant catalogs (10) ──────────────────────────────────────────
    public DbSet<ManagingAuthority> ManagingAuthorities => Set<ManagingAuthority>();
    public DbSet<NationalTargetProgram> NationalTargetPrograms => Set<NationalTargetProgram>();
    public DbSet<ProjectOwner> ProjectOwners => Set<ProjectOwner>();
    public DbSet<ProjectManagementUnit> ProjectManagementUnits => Set<ProjectManagementUnit>();
    public DbSet<InvestmentDecisionAuthority> InvestmentDecisionAuthorities => Set<InvestmentDecisionAuthority>();
    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<ProjectImplementationStatus> ProjectImplementationStatuses => Set<ProjectImplementationStatus>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<ManagingAgency> ManagingAgencies => Set<ManagingAgency>();

    // ── KHLCNT ────────────────────────────────────────────────────────────────
    public DbSet<ContractorSelectionPlan> ContractorSelectionPlans => Set<ContractorSelectionPlan>();

    // ── Phase 2 catalogs ──────────────────────────────────────────────────────
    public DbSet<GovernmentAgency> GovernmentAgencies => Set<GovernmentAgency>();
    public DbSet<Investor> Investors => Set<Investor>();

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
            // Phase 2: lifecycle fields
            e.Property(x => x.Status).HasDefaultValue(AdministrativeStatus.Active);
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
            // Phase 2: lifecycle fields
            e.Property(x => x.Status).HasDefaultValue(AdministrativeStatus.Active);
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

        // ── Phase 2: GovernmentAgency (hierarchical) ──────────────────────────
        modelBuilder.Entity<GovernmentAgency>(e =>
        {
            e.ToTable("GovernmentAgencies");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(500).IsRequired();
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.AgencyType).HasMaxLength(100);
            e.Property(x => x.Origin).HasMaxLength(200);
            e.Property(x => x.LdaServer).HasMaxLength(200);
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Fax).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => new { x.Code, x.TenantId }).IsUnique();
            e.HasIndex(x => x.TenantId);
            // Self-referencing hierarchy
            e.HasOne(x => x.Parent).WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Phase 2: Investor (flat) ──────────────────────────────────────────
        modelBuilder.Entity<Investor>(e =>
        {
            e.ToTable("Investors");
            e.HasKey(x => x.Id);
            e.Property(x => x.InvestorType).HasMaxLength(100).IsRequired();
            e.Property(x => x.BusinessIdOrCccd).HasMaxLength(50).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(500).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(500);
            e.HasIndex(x => new { x.BusinessIdOrCccd, x.TenantId }).IsUnique();
            e.HasIndex(x => x.TenantId);
        });

        // ── New GSDT catalog configurations ───────────────────────────────────
        SeedCatalogConfigurations.Configure(modelBuilder);
        DynamicCatalogConfigurations.Configure(modelBuilder);
        ContractorSelectionPlanConfiguration.Configure(modelBuilder);
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
            // Self-referencing tree
            e.HasOne(x => x.Parent).WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Dictionary).WithMany(x => x.Items)
                .HasForeignKey(x => x.DictionaryId).OnDelete(DeleteBehavior.Cascade);
            // Note: DictionaryItemVersion removed (YAGNI)
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
