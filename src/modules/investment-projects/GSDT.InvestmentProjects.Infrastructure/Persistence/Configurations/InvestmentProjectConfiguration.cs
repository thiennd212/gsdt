namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for the InvestmentProject TPT hierarchy.
/// Base table: investment.InvestmentProjects
/// Child tables: investment.DomesticProjects, investment.OdaProjects
/// </summary>
internal sealed class InvestmentProjectConfiguration : IEntityTypeConfiguration<InvestmentProject>
{
    public void Configure(EntityTypeBuilder<InvestmentProject> builder)
    {
        builder.ToTable("InvestmentProjects");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProjectName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ProjectType).IsRequired();
        builder.Property(x => x.PmuDirectorName).HasMaxLength(200);
        builder.Property(x => x.PmuPhone).HasMaxLength(20);
        builder.Property(x => x.PmuEmail).HasMaxLength(200);
        builder.Property(x => x.ImplementationPeriod).HasMaxLength(200);
        builder.Property(x => x.PolicyDecisionNumber).HasMaxLength(100);
        builder.Property(x => x.PolicyDecisionAuthority).HasMaxLength(200);
        builder.Property(x => x.PolicyDecisionPerson).HasMaxLength(200);
        builder.Property(x => x.RowVersion).IsRowVersion();

        // Unique: ProjectCode per tenant
        builder.HasIndex(x => new { x.ProjectCode, x.TenantId }).IsUnique();

        // Shared children (both domestic + ODA)
        builder.HasMany(x => x.Locations)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.BidPackages)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.InspectionRecords)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.EvaluationRecords)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AuditRecords)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ViolationRecords)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Documents)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-to-1: OperationInfo (shared PK)
        builder.HasOne(x => x.OperationInfo)
            .WithOne(x => x.Project).HasForeignKey<OperationInfo>(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>TPT child table for domestic public investment projects.</summary>
internal sealed class DomesticProjectConfiguration : IEntityTypeConfiguration<DomesticProject>
{
    public void Configure(EntityTypeBuilder<DomesticProject> builder)
    {
        builder.ToTable("DomesticProjects"); // TPT — own table

        builder.Property(x => x.TreasuryCode).HasMaxLength(50);
        builder.Property(x => x.StopContent).HasMaxLength(1000);
        builder.Property(x => x.StopDecisionNumber).HasMaxLength(100);
        builder.Property(x => x.PrelimCentralBudget).HasPrecision(18, 4);
        builder.Property(x => x.PrelimLocalBudget).HasPrecision(18, 4);
        builder.Property(x => x.PrelimOtherPublicCapital).HasPrecision(18, 4);
        builder.Property(x => x.PrelimPublicInvestment).HasPrecision(18, 4);
        builder.Property(x => x.PrelimOtherCapital).HasPrecision(18, 4);
        builder.Property(x => x.PrelimTotalInvestment).HasPrecision(18, 4);

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
    }
}

/// <summary>TPT child table for ODA-funded investment projects.</summary>
internal sealed class OdaProjectConfiguration : IEntityTypeConfiguration<OdaProject>
{
    public void Configure(EntityTypeBuilder<OdaProject> builder)
    {
        builder.ToTable("OdaProjects"); // TPT — own table

        builder.Property(x => x.ShortName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProjectCodeQhns).HasMaxLength(100);
        builder.Property(x => x.CoDonorName).HasMaxLength(200);
        builder.Property(x => x.ProcurementConditionSummary).HasMaxLength(1000);
        builder.Property(x => x.OdaGrantCapital).HasPrecision(18, 4);
        builder.Property(x => x.OdaLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartCentralBudget).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartLocalBudget).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartOtherCapital).HasPrecision(18, 4);
        builder.Property(x => x.TotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.GrantMechanismPercent).HasPrecision(18, 4);
        builder.Property(x => x.RelendingMechanismPercent).HasPrecision(18, 4);

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

        builder.HasMany(x => x.LoanAgreements)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ServiceBanks)
            .WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-to-1: ProcurementCondition (shared PK)
        builder.HasOne(x => x.ProcurementCondition)
            .WithOne(x => x.Project).HasForeignKey<ProcurementCondition>(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
