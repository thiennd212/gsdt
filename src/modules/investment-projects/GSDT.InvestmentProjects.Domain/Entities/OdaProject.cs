namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// ODA-funded investment project (TPT child table: investment.OdaProjects).
/// Extends InvestmentProject with donor, grant/loan capital, and procurement condition fields.
/// </summary>
public sealed class OdaProject : InvestmentProject
{
    /// <summary>Short/abbreviated project name for reporting (max 200 chars).</summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>QHNS project code — optional, added in v1.1 (max 100 chars).</summary>
    public string? ProjectCodeQhns { get; set; }

    /// <summary>Guid ref to MasterData: ODA project type (grant, loan, mixed).</summary>
    public Guid OdaProjectTypeId { get; set; }

    /// <summary>Guid ref to MasterData: primary donor organisation.</summary>
    public Guid DonorId { get; set; }

    /// <summary>Name of co-donor(s) if applicable (max 200 chars).</summary>
    public string? CoDonorName { get; set; }

    // Capital breakdown — precision (18,4)
    public decimal OdaGrantCapital { get; set; }
    public decimal OdaLoanCapital { get; set; }
    public decimal CounterpartCentralBudget { get; set; }
    public decimal CounterpartLocalBudget { get; set; }
    public decimal CounterpartOtherCapital { get; set; }
    public decimal TotalInvestment { get; set; }

    // Mechanism percentages — precision (18,4)
    public decimal GrantMechanismPercent { get; set; }
    public decimal RelendingMechanismPercent { get; set; }

    /// <summary>Guid ref to MasterData: current project status.</summary>
    public Guid StatusId { get; set; }

    // Procurement conditions
    public bool ProcurementConditionBound { get; set; }
    public string? ProcurementConditionSummary { get; set; }

    public int? StartYear { get; set; }
    public int? EndYear { get; set; }

    // Navigation properties specific to ODA projects
    public ICollection<OdaInvestmentDecision> InvestmentDecisions { get; set; } = new List<OdaInvestmentDecision>();
    public ICollection<OdaCapitalPlan> CapitalPlans { get; set; } = new List<OdaCapitalPlan>();
    public ICollection<OdaExecutionRecord> ExecutionRecords { get; set; } = new List<OdaExecutionRecord>();
    public ICollection<OdaDisbursementRecord> DisbursementRecords { get; set; } = new List<OdaDisbursementRecord>();
    public ICollection<LoanAgreement> LoanAgreements { get; set; } = new List<LoanAgreement>();
    public ICollection<ServiceBank> ServiceBanks { get; set; } = new List<ServiceBank>();
    public ProcurementCondition? ProcurementCondition { get; set; }

    private OdaProject() { } // EF Core

    /// <summary>Factory method — raises ProjectCreatedEvent.</summary>
    public static OdaProject Create(
        Guid tenantId,
        string projectCode,
        string projectName,
        string shortName,
        Guid managingAuthorityId,
        Guid industrySectorId,
        Guid projectOwnerId,
        Guid odaProjectTypeId,
        Guid donorId)
    {
        var project = new OdaProject
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectCode = projectCode,
            ProjectName = projectName,
            ShortName = shortName,
            ProjectType = ProjectType.Oda,
            ManagingAuthorityId = managingAuthorityId,
            IndustrySectorId = industrySectorId,
            ProjectOwnerId = projectOwnerId,
            OdaProjectTypeId = odaProjectTypeId,
            DonorId = donorId
        };
        project.RaiseCreatedEvent();
        return project;
    }
}
