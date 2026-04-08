using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>EF configurations for: ProjectLocation, decisions, capital plans, execution records,
/// inspection/evaluation/audit/violation records, operation info, project document,
/// loan agreement, service bank, procurement condition.</summary>

internal sealed class ProjectLocationConfiguration : IEntityTypeConfiguration<ProjectLocation>
{
    public void Configure(EntityTypeBuilder<ProjectLocation> builder)
    {
        builder.ToTable("ProjectLocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.IndustrialZoneName).HasMaxLength(500);
    }
}

internal sealed class DomesticInvestmentDecisionConfiguration
    : IEntityTypeConfiguration<DomesticInvestmentDecision>
{
    public void Configure(EntityTypeBuilder<DomesticInvestmentDecision> builder)
    {
        builder.ToTable("DomesticInvestmentDecisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DecisionNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DecisionAuthority).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.TotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.CentralBudget).HasPrecision(18, 4);
        builder.Property(x => x.LocalBudget).HasPrecision(18, 4);
        builder.Property(x => x.OtherPublicCapital).HasPrecision(18, 4);
        builder.Property(x => x.OtherCapital).HasPrecision(18, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class OdaInvestmentDecisionConfiguration
    : IEntityTypeConfiguration<OdaInvestmentDecision>
{
    public void Configure(EntityTypeBuilder<OdaInvestmentDecision> builder)
    {
        builder.ToTable("OdaInvestmentDecisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DecisionNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DecisionAuthority).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.OdaGrantCapital).HasPrecision(18, 4);
        builder.Property(x => x.OdaLoanCapital).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartCentralBudget).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartLocalBudget).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartOtherCapital).HasPrecision(18, 4);
        builder.Property(x => x.TotalInvestment).HasPrecision(18, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class DomesticCapitalPlanConfiguration : IEntityTypeConfiguration<DomesticCapitalPlan>
{
    public void Configure(EntityTypeBuilder<DomesticCapitalPlan> builder)
    {
        builder.ToTable("DomesticCapitalPlans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DecisionNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.CentralBudget).HasPrecision(18, 4);
        builder.Property(x => x.LocalBudget).HasPrecision(18, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class OdaCapitalPlanConfiguration : IEntityTypeConfiguration<OdaCapitalPlan>
{
    public void Configure(EntityTypeBuilder<OdaCapitalPlan> builder)
    {
        builder.ToTable("OdaCapitalPlans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DecisionNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.OdaGrant).HasPrecision(18, 4);
        builder.Property(x => x.OdaLoan).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartCentral).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartLocal).HasPrecision(18, 4);
        builder.Property(x => x.CounterpartOther).HasPrecision(18, 4);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class DomesticExecutionRecordConfiguration
    : IEntityTypeConfiguration<DomesticExecutionRecord>
{
    public void Configure(EntityTypeBuilder<DomesticExecutionRecord> builder)
    {
        builder.ToTable("DomesticExecutionRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.PhysicalProgressPercent).HasPrecision(18, 4);
    }
}

internal sealed class OdaExecutionRecordConfiguration : IEntityTypeConfiguration<OdaExecutionRecord>
{
    public void Configure(EntityTypeBuilder<OdaExecutionRecord> builder)
    {
        builder.ToTable("OdaExecutionRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.PhysicalProgressPercent).HasPrecision(18, 4);
        builder.Property(x => x.CumulativeFromStart).HasPrecision(18, 4);
    }
}

internal sealed class InspectionRecordConfiguration : IEntityTypeConfiguration<InspectionRecord>
{
    public void Configure(EntityTypeBuilder<InspectionRecord> builder)
    {
        builder.ToTable("InspectionRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InspectionAgency).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Conclusion).HasMaxLength(2000);
    }
}

internal sealed class EvaluationRecordConfiguration : IEntityTypeConfiguration<EvaluationRecord>
{
    public void Configure(EntityTypeBuilder<EvaluationRecord> builder)
    {
        builder.ToTable("EvaluationRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Result).HasMaxLength(2000);
    }
}

internal sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("AuditRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AuditAgency).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(2000).IsRequired();
    }
}

internal sealed class ViolationRecordConfiguration : IEntityTypeConfiguration<ViolationRecord>
{
    public void Configure(EntityTypeBuilder<ViolationRecord> builder)
    {
        builder.ToTable("ViolationRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Penalty).HasPrecision(18, 4);
    }
}

internal sealed class OperationInfoConfiguration : IEntityTypeConfiguration<OperationInfo>
{
    public void Configure(EntityTypeBuilder<OperationInfo> builder)
    {
        builder.ToTable("OperationInfos");
        // Shared PK with InvestmentProject
        builder.HasKey(x => x.ProjectId);
        builder.Property(x => x.OperatingAgency).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.RevenueLastYear).HasPrecision(18, 4);
        builder.Property(x => x.ExpenseLastYear).HasPrecision(18, 4);
    }
}

internal sealed class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("ProjectDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);
    }
}

internal sealed class LoanAgreementConfiguration : IEntityTypeConfiguration<LoanAgreement>
{
    public void Configure(EntityTypeBuilder<LoanAgreement> builder)
    {
        builder.ToTable("LoanAgreements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AgreementNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LenderName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.InterestRate).HasPrecision(18, 4);
    }
}

internal sealed class ServiceBankConfiguration : IEntityTypeConfiguration<ServiceBank>
{
    public void Configure(EntityTypeBuilder<ServiceBank> builder)
    {
        builder.ToTable("ServiceBanks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);
    }
}

internal sealed class ProcurementConditionConfiguration : IEntityTypeConfiguration<ProcurementCondition>
{
    public void Configure(EntityTypeBuilder<ProcurementCondition> builder)
    {
        builder.ToTable("ProcurementConditions");
        // Shared PK with OdaProject
        builder.HasKey(x => x.ProjectId);
        builder.Property(x => x.Summary).HasMaxLength(2000);
        builder.Property(x => x.SpecialConditions).HasMaxLength(2000);
    }
}
