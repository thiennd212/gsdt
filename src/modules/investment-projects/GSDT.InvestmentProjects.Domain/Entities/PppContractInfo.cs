namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// PPP contract information — 1-to-1 with PppProject.
/// ProjectId is both PK and FK (shared primary key pattern).
/// Stores finalized contract capital, dates, and terms.
/// </summary>
public sealed class PppContractInfo : AuditableEntity<Guid>, ITenantScoped
{
    /// <summary>Same value as parent PppProject.Id — shared PK/FK.</summary>
    public Guid ProjectId { get; set; }

    public Guid TenantId { get; set; }

    // Finalized contract capital breakdown — precision (18,4)
    public decimal TotalInvestment { get; set; }
    public decimal StateCapital { get; set; }
    public decimal CentralBudget { get; set; }
    public decimal LocalBudget { get; set; }
    public decimal OtherStateBudget { get; set; }
    public decimal EquityCapital { get; set; }
    public decimal LoanCapital { get; set; }

    /// <summary>Equity capital ratio as percentage — precision (18,4).</summary>
    public decimal? EquityRatio { get; set; }

    /// <summary>Project implementation progress description (max 1000).</summary>
    public string? ImplementationProgress { get; set; }

    /// <summary>Contract duration description (max 200).</summary>
    public string? ContractDuration { get; set; }

    /// <summary>Revenue sharing mechanism description (max 2000).</summary>
    public string? RevenueSharingMechanism { get; set; }

    /// <summary>Authority that signed the contract (max 200).</summary>
    public string? ContractAuthority { get; set; }

    /// <summary>Contract reference number (max 100).</summary>
    public string? ContractNumber { get; set; }

    public DateTime? ContractDate { get; set; }
    public DateTime? ConstructionStartDate { get; set; }
    public DateTime? CompletionDate { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public PppProject Project { get; set; } = default!;

    private PppContractInfo() { } // EF Core

    public static PppContractInfo Create(Guid projectId, Guid tenantId,
        decimal totalInvestment, decimal stateCapital, decimal centralBudget,
        decimal localBudget, decimal otherStateBudget, decimal equityCapital, decimal loanCapital)
        => new()
        {
            ProjectId = projectId,
            TenantId = tenantId,
            TotalInvestment = totalInvestment,
            StateCapital = stateCapital,
            CentralBudget = centralBudget,
            LocalBudget = localBudget,
            OtherStateBudget = otherStateBudget,
            EquityCapital = equityCapital,
            LoanCapital = loanCapital
        };
}
