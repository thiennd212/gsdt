namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Disbursement snapshot for a PPP project at a point in time.
/// Tracks state capital, equity capital, and loan capital — period and cumulative.
/// All monetary fields precision (18,4).
/// </summary>
public sealed class PppDisbursementRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime ReportDate { get; set; }

    // State capital
    public decimal StateCapitalPeriod { get; set; }
    public decimal StateCapitalCumulative { get; set; }

    // Equity capital (investor contribution)
    public decimal EquityCapitalPeriod { get; set; }
    public decimal EquityCapitalCumulative { get; set; }

    // Loan capital
    public decimal LoanCapitalPeriod { get; set; }
    public decimal LoanCapitalCumulative { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public PppProject Project { get; set; } = default!;

    private PppDisbursementRecord() { } // EF Core

    public static PppDisbursementRecord Create(
        Guid tenantId, Guid projectId, DateTime reportDate,
        decimal stateCapitalPeriod, decimal stateCapitalCumulative,
        decimal equityCapitalPeriod, decimal equityCapitalCumulative,
        decimal loanCapitalPeriod, decimal loanCapitalCumulative)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            ReportDate = reportDate,
            StateCapitalPeriod = stateCapitalPeriod,
            StateCapitalCumulative = stateCapitalCumulative,
            EquityCapitalPeriod = equityCapitalPeriod,
            EquityCapitalCumulative = equityCapitalCumulative,
            LoanCapitalPeriod = loanCapitalPeriod,
            LoanCapitalCumulative = loanCapitalCumulative
        };
}
