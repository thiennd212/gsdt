namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Investment decision record for PPP projects (initial or adjustment).</summary>
public sealed class PppInvestmentDecision : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public InvestmentDecisionType DecisionType { get; set; }
    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }
    public string DecisionAuthority { get; set; } = string.Empty;

    /// <summary>Signer of the decision document (max 200, optional).</summary>
    public string? DecisionPerson { get; set; }

    // Capital breakdown — precision (18,4)
    public decimal TotalInvestment { get; set; }
    public decimal StateCapital { get; set; }
    public decimal CentralBudget { get; set; }
    public decimal LocalBudget { get; set; }
    public decimal OtherStateBudget { get; set; }
    public decimal EquityCapital { get; set; }
    public decimal LoanCapital { get; set; }

    /// <summary>Equity capital ratio as percentage — precision (18,4).</summary>
    public decimal? EquityRatio { get; set; }

    /// <summary>Guid ref to MasterData: adjustment content type (used when DecisionType=Adjustment).</summary>
    public Guid? AdjustmentContentId { get; set; }

    /// <summary>Additional notes (max 2000).</summary>
    public string? Notes { get; set; }

    /// <summary>Guid ref to Files module: attached decision document.</summary>
    public Guid? FileId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public PppProject Project { get; set; } = default!;

    private PppInvestmentDecision() { } // EF Core

    public static PppInvestmentDecision Create(
        Guid tenantId, Guid projectId,
        InvestmentDecisionType decisionType,
        string decisionNumber, DateTime decisionDate, string decisionAuthority,
        decimal totalInvestment, decimal stateCapital, decimal centralBudget,
        decimal localBudget, decimal otherStateBudget, decimal equityCapital, decimal loanCapital,
        string? decisionPerson = null, decimal? equityRatio = null,
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
            DecisionPerson = decisionPerson,
            TotalInvestment = totalInvestment,
            StateCapital = stateCapital,
            CentralBudget = centralBudget,
            LocalBudget = localBudget,
            OtherStateBudget = otherStateBudget,
            EquityCapital = equityCapital,
            LoanCapital = loanCapital,
            EquityRatio = equityRatio,
            AdjustmentContentId = adjustmentContentId,
            Notes = notes,
            FileId = fileId
        };
}
