using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configurations for domestic and ODA disbursement records.
/// DomesticDisbursementRecord unique constraint: (ProjectId, ReportDate).
/// OdaDisbursementRecord unique constraint: (ProjectId, ReportDate, BidPackageId, ContractId).
/// All monetary fields: precision (18,4).
/// </summary>
internal sealed class DomesticDisbursementRecordConfiguration
    : IEntityTypeConfiguration<DomesticDisbursementRecord>
{
    public void Configure(EntityTypeBuilder<DomesticDisbursementRecord> builder)
    {
        builder.ToTable("DomesticDisbursementRecords");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PublicCapitalMonthly).HasPrecision(18, 4);
        builder.Property(x => x.PublicCapitalPreviousMonth).HasPrecision(18, 4);
        builder.Property(x => x.PublicCapitalYtd).HasPrecision(18, 4);
        builder.Property(x => x.OtherCapital).HasPrecision(18, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();

        // One record per project per report date
        builder.HasIndex(x => new { x.ProjectId, x.ReportDate }).IsUnique();
    }
}

internal sealed class OdaDisbursementRecordConfiguration
    : IEntityTypeConfiguration<OdaDisbursementRecord>
{
    public void Configure(EntityTypeBuilder<OdaDisbursementRecord> builder)
    {
        builder.ToTable("OdaDisbursementRecords");
        builder.HasKey(x => x.Id);

        // Monthly
        builder.Property(x => x.MonthlyTotal).HasPrecision(18, 4);
        builder.Property(x => x.MonthlyOdaGrant).HasPrecision(18, 4);
        builder.Property(x => x.MonthlyOdaRelending).HasPrecision(18, 4);
        builder.Property(x => x.MonthlyCounterpart).HasPrecision(18, 4);
        builder.Property(x => x.MonthlyCpNstw).HasPrecision(18, 4);
        builder.Property(x => x.MonthlyCpNsdp).HasPrecision(18, 4);
        builder.Property(x => x.MonthlyCpOther).HasPrecision(18, 4);

        // YTD
        builder.Property(x => x.YtdTotal).HasPrecision(18, 4);
        builder.Property(x => x.YtdOdaGrant).HasPrecision(18, 4);
        builder.Property(x => x.YtdOdaRelending).HasPrecision(18, 4);
        builder.Property(x => x.YtdCounterpart).HasPrecision(18, 4);
        builder.Property(x => x.YtdCpNstw).HasPrecision(18, 4);
        builder.Property(x => x.YtdCpNsdp).HasPrecision(18, 4);
        builder.Property(x => x.YtdCpOther).HasPrecision(18, 4);

        // Project-to-date
        builder.Property(x => x.ProjectTotal).HasPrecision(18, 4);
        builder.Property(x => x.ProjectOdaGrant).HasPrecision(18, 4);
        builder.Property(x => x.ProjectOdaRelending).HasPrecision(18, 4);
        builder.Property(x => x.ProjectCounterpart).HasPrecision(18, 4);
        builder.Property(x => x.ProjectCpNstw).HasPrecision(18, 4);
        builder.Property(x => x.ProjectCpNsdp).HasPrecision(18, 4);
        builder.Property(x => x.ProjectCpOther).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();

        // Unique per (project, date, bid package, contract)
        builder.HasIndex(x => new { x.ProjectId, x.ReportDate, x.BidPackageId, x.ContractId })
            .IsUnique();
    }
}
