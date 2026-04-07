namespace GSDT.MasterData.Persistence;

/// <summary>
/// EF Core model configuration for ContractorSelectionPlan (KHLCNT).
/// Tenant-scoped, auditable. OrderNumber is unique per tenant.
/// Called from MasterDataDbContext.OnModelCreating.
/// </summary>
internal static class ContractorSelectionPlanConfiguration
{
    public static void Configure(ModelBuilder mb)
    {
        mb.Entity<ContractorSelectionPlan>(e =>
        {
            e.ToTable("ContractorSelectionPlans");
            e.HasKey(x => x.Id);
            e.Property(x => x.TenantId).IsRequired();
            e.Property(x => x.NameVi).HasMaxLength(500).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(500).IsRequired();
            e.Property(x => x.SignedBy).HasMaxLength(200).IsRequired();
            e.Property(x => x.SignedDate).IsRequired();
            // OrderNumber must be unique within a tenant
            e.HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique();
            e.HasIndex(x => x.TenantId);
        });
    }
}
