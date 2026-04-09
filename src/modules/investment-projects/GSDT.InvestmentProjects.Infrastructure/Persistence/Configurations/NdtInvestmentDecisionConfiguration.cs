namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for NdtInvestmentDecision — investment.NdtInvestmentDecisions.
/// Capital structure: CSH + ODA loan + TCTD loan (same as DNNN).
/// </summary>
internal sealed class NdtInvestmentDecisionConfiguration : IEntityTypeConfiguration<NdtInvestmentDecision>
{
    public void Configure(EntityTypeBuilder<NdtInvestmentDecision> builder)
    {
        builder.ToTable("NdtInvestmentDecisions");

        builder.Property(x => x.DecisionNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DecisionAuthority).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DecisionPerson).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Capital — precision (18,4)
        builder.Property(x => x.TotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.EquityCapital).HasPrecision(18, 4);
        builder.Property(x => x.OdaLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.CreditLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.EquityRatio).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.ProjectId);
    }
}
