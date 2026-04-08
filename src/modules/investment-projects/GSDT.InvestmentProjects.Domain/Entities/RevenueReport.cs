namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Revenue sharing report for a PPP project — tracks actual revenue vs. contract projections.
/// Period and cumulative revenue, plus deviation sharing amounts.
/// All monetary fields precision (18,4).
/// </summary>
public sealed class RevenueReport : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public int ReportYear { get; set; }

    /// <summary>Report period label, e.g. "Q1", "H1", "Annual" (max 50).</summary>
    public string ReportPeriod { get; set; } = string.Empty;

    /// <summary>Revenue generated this period — precision (18,4).</summary>
    public decimal RevenuePeriod { get; set; }

    /// <summary>Revenue generated cumulative from project start — precision (18,4).</summary>
    public decimal RevenueCumulative { get; set; }

    /// <summary>Revenue increase sharing amount (state portion) — precision (18,4).</summary>
    public decimal? RevenueIncreaseSharing { get; set; }

    /// <summary>Revenue decrease sharing amount (investor compensation) — precision (18,4).</summary>
    public decimal? RevenueDecreaseSharing { get; set; }

    /// <summary>Reported difficulties during the period (max 2000).</summary>
    public string? Difficulties { get; set; }

    /// <summary>Recommendations for resolution (max 2000).</summary>
    public string? Recommendations { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public PppProject Project { get; set; } = default!;

    private RevenueReport() { } // EF Core

    public static RevenueReport Create(
        Guid tenantId, Guid projectId,
        int reportYear, string reportPeriod,
        decimal revenuePeriod, decimal revenueCumulative,
        decimal? revenueIncreaseSharing = null, decimal? revenueDecreaseSharing = null,
        string? difficulties = null, string? recommendations = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            ReportYear = reportYear,
            ReportPeriod = reportPeriod,
            RevenuePeriod = revenuePeriod,
            RevenueCumulative = revenueCumulative,
            RevenueIncreaseSharing = revenueIncreaseSharing,
            RevenueDecreaseSharing = revenueDecreaseSharing,
            Difficulties = difficulties,
            Recommendations = recommendations
        };
}
