namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Capital allocation plan decision for an ODA project.</summary>
public sealed class OdaCapitalPlan : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public CapitalDecisionType DecisionType { get; set; }

    /// <summary>Allocation round number (1-based).</summary>
    public int AllocationRound { get; set; }

    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }

    // ODA capital breakdown — precision (18,4)
    public decimal OdaGrant { get; set; }
    public decimal OdaLoan { get; set; }
    public decimal CounterpartCentral { get; set; }
    public decimal CounterpartLocal { get; set; }
    public decimal CounterpartOther { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    /// <summary>Guid ref to Files module: attached plan document.</summary>
    public Guid? FileId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public OdaProject Project { get; set; } = default!;

    private OdaCapitalPlan() { } // EF Core
}
