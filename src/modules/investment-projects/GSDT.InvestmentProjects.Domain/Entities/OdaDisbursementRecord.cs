namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Full disbursement snapshot for an ODA project — monthly, YTD, and project-to-date totals
/// split across ODA grant, ODA relending, and counterpart funding components.
/// Unique constraint: (ProjectId, ReportDate, BidPackageId, ContractId).
/// All monetary fields precision (18,4).
/// </summary>
public sealed class OdaDisbursementRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public DateTime ReportDate { get; set; }
    public Guid? BidPackageId { get; set; }
    public Guid? ContractId { get; set; }

    // Monthly totals
    public decimal MonthlyTotal { get; set; }
    public decimal MonthlyOdaGrant { get; set; }
    public decimal MonthlyOdaRelending { get; set; }
    public decimal MonthlyCounterpart { get; set; }
    public decimal MonthlyCpNstw { get; set; }   // Counterpart: central treasury (NSTW)
    public decimal MonthlyCpNsdp { get; set; }   // Counterpart: local budget (NSDP)
    public decimal MonthlyCpOther { get; set; }  // Counterpart: other sources

    // Year-to-date totals
    public decimal YtdTotal { get; set; }
    public decimal YtdOdaGrant { get; set; }
    public decimal YtdOdaRelending { get; set; }
    public decimal YtdCounterpart { get; set; }
    public decimal YtdCpNstw { get; set; }
    public decimal YtdCpNsdp { get; set; }
    public decimal YtdCpOther { get; set; }

    // Project-to-date (cumulative from start) totals
    public decimal ProjectTotal { get; set; }
    public decimal ProjectOdaGrant { get; set; }
    public decimal ProjectOdaRelending { get; set; }
    public decimal ProjectCounterpart { get; set; }
    public decimal ProjectCpNstw { get; set; }
    public decimal ProjectCpNsdp { get; set; }
    public decimal ProjectCpOther { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public OdaProject Project { get; set; } = default!;

    private OdaDisbursementRecord() { } // EF Core
}
