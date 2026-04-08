namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>EF configurations for PPP-specific child entities:
/// PppInvestmentDecision, PppCapitalPlan, PppExecutionRecord,
/// PppDisbursementRecord, RevenueReport, PppContractInfo.</summary>

internal sealed class PppInvestmentDecisionConfiguration
    : IEntityTypeConfiguration<PppInvestmentDecision>
{
    public void Configure(EntityTypeBuilder<PppInvestmentDecision> builder)
    {
        builder.ToTable("PppInvestmentDecisions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DecisionNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DecisionAuthority).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DecisionPerson).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.TotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.StateCapital).HasPrecision(18, 4);
        builder.Property(x => x.CentralBudget).HasPrecision(18, 4);
        builder.Property(x => x.LocalBudget).HasPrecision(18, 4);
        builder.Property(x => x.OtherStateBudget).HasPrecision(18, 4);
        builder.Property(x => x.EquityCapital).HasPrecision(18, 4);
        builder.Property(x => x.LoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.EquityRatio).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class PppCapitalPlanConfiguration : IEntityTypeConfiguration<PppCapitalPlan>
{
    public void Configure(EntityTypeBuilder<PppCapitalPlan> builder)
    {
        builder.ToTable("PppCapitalPlans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DecisionType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DecisionNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.Property(x => x.StateCapitalByDecision).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class PppExecutionRecordConfiguration : IEntityTypeConfiguration<PppExecutionRecord>
{
    public void Configure(EntityTypeBuilder<PppExecutionRecord> builder)
    {
        builder.ToTable("PppExecutionRecords");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ValueExecutedPeriod).HasPrecision(18, 4);
        builder.Property(x => x.ValueExecutedCumulative).HasPrecision(18, 4);
        builder.Property(x => x.CumulativeFromStart).HasPrecision(18, 4);
        builder.Property(x => x.SubProjectStateCapitalPeriod).HasPrecision(18, 4);
        builder.Property(x => x.SubProjectStateCapitalCumulative).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class PppDisbursementRecordConfiguration
    : IEntityTypeConfiguration<PppDisbursementRecord>
{
    public void Configure(EntityTypeBuilder<PppDisbursementRecord> builder)
    {
        builder.ToTable("PppDisbursementRecords");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StateCapitalPeriod).HasPrecision(18, 4);
        builder.Property(x => x.StateCapitalCumulative).HasPrecision(18, 4);
        builder.Property(x => x.EquityCapitalPeriod).HasPrecision(18, 4);
        builder.Property(x => x.EquityCapitalCumulative).HasPrecision(18, 4);
        builder.Property(x => x.LoanCapitalPeriod).HasPrecision(18, 4);
        builder.Property(x => x.LoanCapitalCumulative).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();

        // One record per project per report date
        builder.HasIndex(x => new { x.ProjectId, x.ReportDate }).IsUnique();
    }
}

internal sealed class RevenueReportConfiguration : IEntityTypeConfiguration<RevenueReport>
{
    public void Configure(EntityTypeBuilder<RevenueReport> builder)
    {
        builder.ToTable("RevenueReports");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReportPeriod).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Difficulties).HasMaxLength(2000);
        builder.Property(x => x.Recommendations).HasMaxLength(2000);

        builder.Property(x => x.RevenuePeriod).HasPrecision(18, 4);
        builder.Property(x => x.RevenueCumulative).HasPrecision(18, 4);
        builder.Property(x => x.RevenueIncreaseSharing).HasPrecision(18, 4);
        builder.Property(x => x.RevenueDecreaseSharing).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class PppContractInfoConfiguration : IEntityTypeConfiguration<PppContractInfo>
{
    public void Configure(EntityTypeBuilder<PppContractInfo> builder)
    {
        builder.ToTable("PppContractInfos");

        // Shared PK with PppProject
        builder.HasKey(x => x.ProjectId);

        builder.Property(x => x.ImplementationProgress).HasMaxLength(1000);
        builder.Property(x => x.ContractDuration).HasMaxLength(200);
        builder.Property(x => x.RevenueSharingMechanism).HasMaxLength(2000);
        builder.Property(x => x.ContractAuthority).HasMaxLength(200);
        builder.Property(x => x.ContractNumber).HasMaxLength(100);

        builder.Property(x => x.TotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.StateCapital).HasPrecision(18, 4);
        builder.Property(x => x.CentralBudget).HasPrecision(18, 4);
        builder.Property(x => x.LocalBudget).HasPrecision(18, 4);
        builder.Property(x => x.OtherStateBudget).HasPrecision(18, 4);
        builder.Property(x => x.EquityCapital).HasPrecision(18, 4);
        builder.Property(x => x.LoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.EquityRatio).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
