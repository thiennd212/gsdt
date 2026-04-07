namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Disbursement snapshot for a domestic project.
/// Unique constraint: (ProjectId, ReportDate) — one record per project per reporting date.
/// All monetary fields precision (18,4).
/// </summary>
public sealed class DomesticDisbursementRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public DateTime ReportDate { get; set; }

    /// <summary>Optional: links disbursement to a specific bid package.</summary>
    public Guid? BidPackageId { get; set; }

    /// <summary>Optional: links disbursement to a specific contract.</summary>
    public Guid? ContractId { get; set; }

    /// <summary>Public capital disbursed this month.</summary>
    public decimal PublicCapitalMonthly { get; set; }

    /// <summary>Public capital disbursed in previous month (for comparison).</summary>
    public decimal? PublicCapitalPreviousMonth { get; set; }

    /// <summary>Public capital disbursed year-to-date.</summary>
    public decimal PublicCapitalYtd { get; set; }

    /// <summary>Other (non-public) capital disbursed.</summary>
    public decimal? OtherCapital { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public DomesticProject Project { get; set; } = default!;

    private DomesticDisbursementRecord() { } // EF Core
}
