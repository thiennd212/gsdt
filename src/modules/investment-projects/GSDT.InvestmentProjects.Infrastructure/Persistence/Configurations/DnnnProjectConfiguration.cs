namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for DnnnProject — TPT child table: investment.DnnnProjects.
/// Configures all DNNN-specific columns and navigations.
/// </summary>
internal sealed class DnnnProjectConfiguration : IEntityTypeConfiguration<DnnnProject>
{
    public void Configure(EntityTypeBuilder<DnnnProject> builder)
    {
        builder.ToTable("DnnnProjects"); // TPT — own child table

        builder.Property(x => x.InvestorName).HasMaxLength(500);
        builder.Property(x => x.StateOwnershipRatio).HasPrecision(5, 2);
        builder.Property(x => x.Objective).HasMaxLength(2000);
        builder.Property(x => x.Capacity).HasMaxLength(500);
        builder.Property(x => x.MainItems).HasMaxLength(2000);
        builder.Property(x => x.ImplementationTimeline).HasMaxLength(200);
        builder.Property(x => x.ProgressDescription).HasMaxLength(1000);
        builder.Property(x => x.StopContent).HasMaxLength(2000);
        builder.Property(x => x.StopDecisionNumber).HasMaxLength(100);

        // Preliminary capital — DNNN structure, precision (18,4)
        builder.Property(x => x.PrelimTotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.PrelimEquityCapital).HasPrecision(18, 4);
        builder.Property(x => x.PrelimOdaLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.PrelimCreditLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.AreaHectares).HasPrecision(18, 4);

        // DNNN-specific children
        builder.HasMany(x => x.InvestmentDecisions)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
