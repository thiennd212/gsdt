namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Loan agreement between an ODA project and a foreign lender.</summary>
public sealed class LoanAgreement : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public string AgreementNumber { get; set; } = string.Empty;
    public DateTime AgreementDate { get; set; }
    public string LenderName { get; set; } = string.Empty;

    /// <summary>Total loan amount — precision (18,4).</summary>
    public decimal Amount { get; set; }

    /// <summary>Currency code (e.g. USD, EUR, JPY — max 10 chars).</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Annual interest rate percentage — precision (18,4).</summary>
    public decimal? InterestRate { get; set; }

    /// <summary>Grace period in years before repayment begins.</summary>
    public int? GracePeriod { get; set; }

    /// <summary>Total repayment period in years.</summary>
    public int? RepaymentPeriod { get; set; }

    public string? Notes { get; set; }

    /// <summary>Guid ref to Files module: signed loan agreement document.</summary>
    public Guid? FileId { get; set; }

    // Navigation
    public OdaProject Project { get; set; } = default!;

    private LoanAgreement() { } // EF Core
}
