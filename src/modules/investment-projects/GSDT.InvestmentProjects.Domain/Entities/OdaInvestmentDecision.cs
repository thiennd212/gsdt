namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Investment decision record for ODA projects (initial or adjustment).</summary>
public sealed class OdaInvestmentDecision : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public InvestmentDecisionType DecisionType { get; set; }
    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }
    public string DecisionAuthority { get; set; } = string.Empty;

    // ODA capital breakdown — precision (18,4)
    public decimal OdaGrantCapital { get; set; }
    public decimal OdaLoanCapital { get; set; }
    public decimal CounterpartCentralBudget { get; set; }
    public decimal CounterpartLocalBudget { get; set; }
    public decimal CounterpartOtherCapital { get; set; }
    public decimal TotalInvestment { get; set; }

    /// <summary>Guid ref to MasterData: adjustment content type.</summary>
    public Guid? AdjustmentContentId { get; set; }
    public string? Notes { get; set; }

    /// <summary>Guid ref to Files module: attached decision document.</summary>
    public Guid? FileId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public OdaProject Project { get; set; } = default!;

    private OdaInvestmentDecision() { } // EF Core
}
