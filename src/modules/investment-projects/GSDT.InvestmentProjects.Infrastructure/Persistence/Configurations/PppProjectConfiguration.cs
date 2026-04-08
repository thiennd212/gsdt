namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for PppProject — TPT child table: investment.PppProjects.
/// Configures all PPP-specific columns and navigations.
/// </summary>
internal sealed class PppProjectConfiguration : IEntityTypeConfiguration<PppProject>
{
    public void Configure(EntityTypeBuilder<PppProject> builder)
    {
        builder.ToTable("PppProjects"); // TPT — own child table

        builder.Property(x => x.PreparationUnit).HasMaxLength(500);
        builder.Property(x => x.Objective).HasMaxLength(2000);
        builder.Property(x => x.Capacity).HasMaxLength(500);
        builder.Property(x => x.MainItems).HasMaxLength(2000);
        builder.Property(x => x.StopContent).HasMaxLength(2000);
        builder.Property(x => x.StopDecisionNumber).HasMaxLength(100);

        // Preliminary capital — precision (18,4)
        builder.Property(x => x.PrelimTotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.PrelimStateCapital).HasPrecision(18, 4);
        builder.Property(x => x.PrelimEquityCapital).HasPrecision(18, 4);
        builder.Property(x => x.PrelimLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.AreaHectares).HasPrecision(18, 4);

        // PPP-specific children
        builder.HasMany(x => x.InvestmentDecisions)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CapitalPlans)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ExecutionRecords)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.DisbursementRecords)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RevenueReports)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-to-1: PppContractInfo (shared PK)
        builder.HasOne(x => x.ContractInfo)
            .WithOne(x => x.Project).HasForeignKey<PppContractInfo>(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
