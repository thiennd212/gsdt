namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Physical execution progress snapshot for an ODA project, including cumulative-from-start figure.</summary>
public sealed class OdaExecutionRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public DateTime ReportDate { get; set; }

    /// <summary>Optional: links progress to a specific bid package.</summary>
    public Guid? BidPackageId { get; set; }

    /// <summary>Optional: links progress to a specific contract.</summary>
    public Guid? ContractId { get; set; }

    public ProgressStatus ProgressStatus { get; set; }

    /// <summary>Physical completion percentage — precision (18,4).</summary>
    public decimal? PhysicalProgressPercent { get; set; }

    /// <summary>Cumulative disbursement from project start — precision (18,4).</summary>
    public decimal? CumulativeFromStart { get; set; }

    public string? Notes { get; set; }

    // Navigation
    public OdaProject Project { get; set; } = default!;

    private OdaExecutionRecord() { } // EF Core
}
