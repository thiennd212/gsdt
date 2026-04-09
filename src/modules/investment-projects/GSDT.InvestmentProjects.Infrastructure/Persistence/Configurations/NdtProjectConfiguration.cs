namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for NdtProject — TPT child table: investment.NdtProjects.
/// Same column config as DNNN (clone minus DesignEstimate/InvestorSelection navs).
/// </summary>
internal sealed class NdtProjectConfiguration : IEntityTypeConfiguration<NdtProject>
{
    public void Configure(EntityTypeBuilder<NdtProject> builder)
    {
        builder.ToTable("NdtProjects"); // TPT — own child table

        builder.Property(x => x.InvestorName).HasMaxLength(500);
        builder.Property(x => x.StateOwnershipRatio).HasPrecision(5, 2);
        builder.Property(x => x.Objective).HasMaxLength(2000);
        builder.Property(x => x.Capacity).HasMaxLength(500);
        builder.Property(x => x.MainItems).HasMaxLength(2000);
        builder.Property(x => x.ImplementationTimeline).HasMaxLength(200);
        builder.Property(x => x.ProgressDescription).HasMaxLength(1000);
        builder.Property(x => x.StopContent).HasMaxLength(2000);
        builder.Property(x => x.StopDecisionNumber).HasMaxLength(100);

        // Preliminary capital — precision (18,4)
        builder.Property(x => x.PrelimTotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.PrelimEquityCapital).HasPrecision(18, 4);
        builder.Property(x => x.PrelimOdaLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.PrelimCreditLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.AreaHectares).HasPrecision(18, 4);

        // NĐT-specific children
        builder.HasMany(x => x.InvestmentDecisions)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
