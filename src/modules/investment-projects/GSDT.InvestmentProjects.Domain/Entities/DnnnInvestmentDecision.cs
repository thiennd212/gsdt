namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Investment decision record for DNNN projects (initial or adjustment).
/// Capital breakdown: CSH + ODA loan + TCTD loan (differs from PPP and TN).
/// </summary>
public sealed class DnnnInvestmentDecision : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public InvestmentDecisionType DecisionType { get; set; }
    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }
    public string DecisionAuthority { get; set; } = string.Empty;

    /// <summary>Signer of the decision document (max 200, optional).</summary>
    public string? DecisionPerson { get; set; }

    // Capital breakdown — DNNN structure, precision (18,4)
    public decimal TotalInvestment { get; set; }
    public decimal EquityCapital { get; set; }
    public decimal OdaLoanCapital { get; set; }
    public decimal CreditLoanCapital { get; set; }

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
    public DnnnProject Project { get; set; } = default!;

    private DnnnInvestmentDecision() { } // EF Core

    public static DnnnInvestmentDecision Create(
        Guid tenantId, Guid projectId,
        InvestmentDecisionType decisionType,
        string decisionNumber, DateTime decisionDate, string decisionAuthority,
        decimal totalInvestment, decimal equityCapital,
        decimal odaLoanCapital, decimal creditLoanCapital,
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
            EquityCapital = equityCapital,
            OdaLoanCapital = odaLoanCapital,
            CreditLoanCapital = creditLoanCapital,
            EquityRatio = equityRatio,
            AdjustmentContentId = adjustmentContentId,
            Notes = notes,
            FileId = fileId
        };
}
