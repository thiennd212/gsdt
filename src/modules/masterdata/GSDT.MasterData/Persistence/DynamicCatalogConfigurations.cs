using GSDT.MasterData.Entities.Catalogs;

namespace GSDT.MasterData.Persistence;

/// <summary>
/// EF Core model configurations for the 10 tenant-scoped dynamic catalogs.
/// Each inherits TenantCatalog (AuditableEntity + ITenantScoped).
/// Unique index is composite (Code, TenantId) — same code can exist across tenants.
/// Called from MasterDataDbContext.OnModelCreating.
/// </summary>
internal static class DynamicCatalogConfigurations
{
    public static void Configure(ModelBuilder mb)
    {
        ConfigureTenantCatalog<ManagingAuthority>(mb, "ManagingAuthorities");
        ConfigureTenantCatalog<NationalTargetProgram>(mb, "NationalTargetPrograms");
        ConfigureTenantCatalog<ProjectOwner>(mb, "ProjectOwners");
        ConfigureTenantCatalog<ProjectManagementUnit>(mb, "ProjectManagementUnits");
        ConfigureTenantCatalog<InvestmentDecisionAuthority>(mb, "InvestmentDecisionAuthorities");
        ConfigureTenantCatalog<Contractor>(mb, "Contractors");
        ConfigureTenantCatalog<DocumentType>(mb, "DocumentTypes");
        ConfigureTenantCatalog<ProjectImplementationStatus>(mb, "ProjectImplementationStatuses");
        ConfigureTenantCatalog<Bank>(mb, "Banks");
        ConfigureTenantCatalog<ManagingAgency>(mb, "ManagingAgencies");
    }

    /// <summary>
    /// Shared EF configuration for all TenantCatalog-derived entities.
    /// AuditableEntity columns (CreatedBy, ModifiedBy, ClassificationLevel) are mapped by convention;
    /// Entity base columns (CreatedAt, UpdatedAt, IsDeleted) handled by ModuleDbContext global filters.
    /// </summary>
    private static void ConfigureTenantCatalog<T>(ModelBuilder mb, string tableName)
        where T : TenantCatalog
    {
        mb.Entity<T>(e =>
        {
            e.ToTable(tableName);
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.TenantId).IsRequired();
            // Composite unique: same code can appear in different tenants
            e.HasIndex(x => new { x.Code, x.TenantId }).IsUnique();
            e.HasIndex(x => x.TenantId);
        });
    }
}
