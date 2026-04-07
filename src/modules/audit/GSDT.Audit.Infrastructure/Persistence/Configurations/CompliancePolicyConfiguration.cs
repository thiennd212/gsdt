
namespace GSDT.Audit.Infrastructure.Persistence.Configurations;

public sealed class CompliancePolicyConfiguration : IEntityTypeConfiguration<CompliancePolicy>
{
    public void Configure(EntityTypeBuilder<CompliancePolicy> b)
    {
        b.ToTable("CompliancePolicies");
        b.HasKey(e => e.Id);

        b.Property(e => e.Name).HasMaxLength(256).IsRequired();
        b.Property(e => e.Rules).HasColumnType("nvarchar(max)").IsRequired();
        b.Property(e => e.Category).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(e => e.Enforcement).HasConversion<string>().HasMaxLength(32).IsRequired();

        b.HasIndex(e => e.IsEnabled);
        b.HasIndex(e => e.Category);
    }
}
