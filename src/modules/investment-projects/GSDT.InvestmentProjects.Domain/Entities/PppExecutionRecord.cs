namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Physical execution progress snapshot for a PPP project at a point in time.
/// Tracks value executed (period/cumulative/from-start) and optional sub-project state capital.
/// All monetary fields precision (18,4).
/// </summary>
public sealed class PppExecutionRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime ReportDate { get; set; }

    /// <summary>Value executed this period — precision (18,4).</summary>
    public decimal ValueExecutedPeriod { get; set; }

    /// <summary>Value executed cumulative (year-to-date) — precision (18,4).</summary>
    public decimal ValueExecutedCumulative { get; set; }

    /// <summary>Value executed cumulative from project start — precision (18,4).</summary>
    public decimal CumulativeFromStart { get; set; }

    /// <summary>Sub-project state capital executed this period — precision (18,4).</summary>
    public decimal? SubProjectStateCapitalPeriod { get; set; }

    /// <summary>Sub-project state capital executed cumulative — precision (18,4).</summary>
    public decimal? SubProjectStateCapitalCumulative { get; set; }

    /// <summary>Optional: links progress to a specific bid package.</summary>
    public Guid? BidPackageId { get; set; }

    /// <summary>Optional: links progress to a specific contract.</summary>
    public Guid? ContractId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public PppProject Project { get; set; } = default!;

    private PppExecutionRecord() { } // EF Core

    public static PppExecutionRecord Create(
        Guid tenantId, Guid projectId, DateTime reportDate,
        decimal valueExecutedPeriod, decimal valueExecutedCumulative, decimal cumulativeFromStart,
        decimal? subProjectStateCapitalPeriod = null, decimal? subProjectStateCapitalCumulative = null,
        Guid? bidPackageId = null, Guid? contractId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            ReportDate = reportDate,
            ValueExecutedPeriod = valueExecutedPeriod,
            ValueExecutedCumulative = valueExecutedCumulative,
            CumulativeFromStart = cumulativeFromStart,
            SubProjectStateCapitalPeriod = subProjectStateCapitalPeriod,
            SubProjectStateCapitalCumulative = subProjectStateCapitalCumulative,
            BidPackageId = bidPackageId,
            ContractId = contractId
        };
}
