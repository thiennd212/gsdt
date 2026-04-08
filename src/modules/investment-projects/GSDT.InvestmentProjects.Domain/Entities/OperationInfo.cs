namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Post-completion operational information for a project — 1-to-1 with InvestmentProject.
/// ProjectId is both PK and FK (shared primary key pattern).
/// </summary>
public sealed class OperationInfo : AuditableEntity<Guid>, ITenantScoped
{
    /// <summary>Same value as the parent InvestmentProject.Id — shared PK/FK.</summary>
    public Guid ProjectId { get; set; }
    public Guid TenantId { get; set; }

    public DateTime? OperationDate { get; set; }
    public string? OperatingAgency { get; set; }

    /// <summary>Revenue in the last full reporting year — precision (18,4).</summary>
    public decimal? RevenueLastYear { get; set; }

    /// <summary>Expenditure in the last full reporting year — precision (18,4).</summary>
    public decimal? ExpenseLastYear { get; set; }

    public string? Notes { get; set; }

    // Navigation
    public InvestmentProject Project { get; set; } = default!;

    private OperationInfo() { } // EF Core
}
