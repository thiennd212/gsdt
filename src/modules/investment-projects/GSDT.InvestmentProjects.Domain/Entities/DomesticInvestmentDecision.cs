namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Investment decision record for domestic projects (initial or adjustment).</summary>
public sealed class DomesticInvestmentDecision : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public InvestmentDecisionType DecisionType { get; set; }
    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }
    public string DecisionAuthority { get; set; } = string.Empty;

    // Capital breakdown — precision (18,4)
    public decimal TotalInvestment { get; set; }
    public decimal CentralBudget { get; set; }
    public decimal LocalBudget { get; set; }
    public decimal OtherPublicCapital { get; set; }
    public decimal OtherCapital { get; set; }

    /// <summary>Guid ref to MasterData: adjustment content type (used when DecisionType=Adjustment).</summary>
    public Guid? AdjustmentContentId { get; set; }
    public string? Notes { get; set; }

    /// <summary>Guid ref to Files module: attached decision document.</summary>
    public Guid? FileId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public DomesticProject Project { get; set; } = default!;

    private DomesticInvestmentDecision() { } // EF Core

    /// <summary>Factory method — use instead of object initializer (Id is protected set).</summary>
    public static DomesticInvestmentDecision Create(
        Guid tenantId, Guid projectId,
        InvestmentDecisionType decisionType,
        string decisionNumber, DateTime decisionDate, string decisionAuthority,
        decimal totalInvestment, decimal centralBudget, decimal localBudget,
        decimal otherPublicCapital, decimal otherCapital,
        Guid? adjustmentContentId = null, string? notes = null, Guid? fileId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            DecisionType = decisionType,
            DecisionNumber = decisionNumber,
            DecisionDate = decisionDate,
            DecisionAuthority = decisionAuthority,
            TotalInvestment = totalInvestment,
            CentralBudget = centralBudget,
            LocalBudget = localBudget,
            OtherPublicCapital = otherPublicCapital,
            OtherCapital = otherCapital,
            AdjustmentContentId = adjustmentContentId,
            Notes = notes,
            FileId = fileId
        };
}
