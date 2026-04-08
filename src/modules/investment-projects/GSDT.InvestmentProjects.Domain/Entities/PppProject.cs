namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Public-Private Partnership project (TPT child table: investment.PppProjects).
/// Extends InvestmentProject with PPP-specific contract and capital fields.
/// </summary>
public sealed class PppProject : InvestmentProject
{
    public PppContractType ContractType { get; set; }
    public SubProjectType SubProjectType { get; set; }

    /// <summary>Guid ref to MasterData: project group classification.</summary>
    public Guid ProjectGroupId { get; set; }

    /// <summary>Guid ref to MasterData: current project status.</summary>
    public Guid StatusId { get; set; }

    /// <summary>Guid ref to MasterData: competent authority for the PPP project (optional).</summary>
    public Guid? CompetentAuthorityId { get; set; }

    /// <summary>Unit responsible for project preparation (max 500).</summary>
    public string? PreparationUnit { get; set; }

    /// <summary>Project objectives and scope description (max 2000).</summary>
    public string? Objective { get; set; }

    // Preliminary capital estimates (pre-decision) — precision (18,4)
    public decimal PrelimTotalInvestment { get; set; }
    public decimal PrelimStateCapital { get; set; }
    public decimal PrelimEquityCapital { get; set; }
    public decimal PrelimLoanCapital { get; set; }

    /// <summary>Total project land area in hectares — precision (18,4).</summary>
    public decimal? AreaHectares { get; set; }

    /// <summary>Project capacity description (max 500).</summary>
    public string? Capacity { get; set; }

    /// <summary>Main construction items description (max 2000).</summary>
    public string? MainItems { get; set; }

    // Suspension/stop fields
    public string? StopContent { get; set; }
    public string? StopDecisionNumber { get; set; }
    public DateTime? StopDecisionDate { get; set; }

    /// <summary>Guid ref to Files module: suspension decision document.</summary>
    public Guid? StopFileId { get; set; }

    // Navigation properties specific to PPP projects
    public ICollection<PppInvestmentDecision> InvestmentDecisions { get; set; } = new List<PppInvestmentDecision>();
    public ICollection<PppCapitalPlan> CapitalPlans { get; set; } = new List<PppCapitalPlan>();
    public ICollection<PppExecutionRecord> ExecutionRecords { get; set; } = new List<PppExecutionRecord>();
    public ICollection<PppDisbursementRecord> DisbursementRecords { get; set; } = new List<PppDisbursementRecord>();
    public ICollection<RevenueReport> RevenueReports { get; set; } = new List<RevenueReport>();
    public PppContractInfo? ContractInfo { get; set; }

    private PppProject() { } // EF Core

    /// <summary>Factory method — raises ProjectCreatedEvent.</summary>
    public static PppProject Create(
        Guid tenantId,
        string projectCode,
        string projectName,
        Guid managingAuthorityId,
        Guid industrySectorId,
        Guid projectOwnerId,
        Guid projectGroupId,
        PppContractType contractType,
        SubProjectType subProjectType = SubProjectType.NotSubProject)
    {
        var project = new PppProject
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectCode = projectCode,
            ProjectName = projectName,
            ProjectType = ProjectType.Ppp,
            ManagingAuthorityId = managingAuthorityId,
            IndustrySectorId = industrySectorId,
            ProjectOwnerId = projectOwnerId,
            ProjectGroupId = projectGroupId,
            ContractType = contractType,
            SubProjectType = subProjectType
        };
        project.RaiseCreatedEvent();
        return project;
    }
}
