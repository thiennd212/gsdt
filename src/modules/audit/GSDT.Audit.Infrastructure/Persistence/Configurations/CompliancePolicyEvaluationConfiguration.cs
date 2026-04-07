
namespace GSDT.Audit.Infrastructure.Persistence.Configurations;

public sealed class CompliancePolicyEvaluationConfiguration : IEntityTypeConfiguration<CompliancePolicyEvaluation>
{
    public void Configure(EntityTypeBuilder<CompliancePolicyEvaluation> b)
    {
        b.ToTable("CompliancePolicyEvaluations");
        b.HasKey(e => e.Id);

        b.Property(e => e.EntityType).HasMaxLength(256).IsRequired();
        b.Property(e => e.Result).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(e => e.Details).HasColumnType("nvarchar(max)");

        b.HasIndex(e => new { e.PolicyId, e.EvaluatedAt });
        b.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}
