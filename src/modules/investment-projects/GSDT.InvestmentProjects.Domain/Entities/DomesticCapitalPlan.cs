namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Capital allocation plan decision for a domestic project.</summary>
public sealed class DomesticCapitalPlan : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public CapitalDecisionType DecisionType { get; set; }

    /// <summary>Allocation round number (1-based, increments with each adjustment).</summary>
    public int AllocationRound { get; set; }

    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }

    // Capital breakdown — precision (18,4)
    public decimal TotalAmount { get; set; }
    public decimal CentralBudget { get; set; }
    public decimal LocalBudget { get; set; }

    public string? Notes { get; set; }

    /// <summary>Guid ref to Files module: attached plan document.</summary>
    public Guid? FileId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public DomesticProject Project { get; set; } = default!;

    private DomesticCapitalPlan() { } // EF Core
}
