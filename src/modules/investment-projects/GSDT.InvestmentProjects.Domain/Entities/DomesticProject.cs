namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Domestic public-investment project (TPT child table: investment.DomesticProjects).
/// Extends InvestmentProject with domestic-specific capital and status fields.
/// </summary>
public sealed class DomesticProject : InvestmentProject
{
    public SubProjectType SubProjectType { get; set; }

    /// <summary>State treasury account code for disbursement tracking.</summary>
    public string? TreasuryCode { get; set; }

    /// <summary>Guid ref to MasterData: project group classification.</summary>
    public Guid ProjectGroupId { get; set; }

    // Preliminary capital estimates (pre-decision) — precision (18,4)
    public decimal PrelimCentralBudget { get; set; }
    public decimal PrelimLocalBudget { get; set; }
    public decimal PrelimOtherPublicCapital { get; set; }
    public decimal PrelimPublicInvestment { get; set; }
    public decimal PrelimOtherCapital { get; set; }
    public decimal PrelimTotalInvestment { get; set; }

    /// <summary>Guid ref to MasterData: current project status (e.g. active, suspended, completed).</summary>
    public Guid StatusId { get; set; }

    // Stop/suspension fields
    public string? StopContent { get; set; }
    public string? StopDecisionNumber { get; set; }
    public DateTime? StopDecisionDate { get; set; }

    /// <summary>Guid ref to Files module: suspension decision document.</summary>
    public Guid? StopFileId { get; set; }

    /// <summary>Guid ref to MasterData: national target program this project belongs to.</summary>
    public Guid? NationalTargetProgramId { get; set; }

    // Navigation properties specific to domestic projects
    public ICollection<DomesticInvestmentDecision> InvestmentDecisions { get; set; } = new List<DomesticInvestmentDecision>();
    public ICollection<DomesticCapitalPlan> CapitalPlans { get; set; } = new List<DomesticCapitalPlan>();
    public ICollection<DomesticExecutionRecord> ExecutionRecords { get; set; } = new List<DomesticExecutionRecord>();
    public ICollection<DomesticDisbursementRecord> DisbursementRecords { get; set; } = new List<DomesticDisbursementRecord>();

    private DomesticProject() { } // EF Core

    /// <summary>Factory method — raises ProjectCreatedEvent.</summary>
    public static DomesticProject Create(
        Guid tenantId,
        string projectCode,
        string projectName,
        Guid managingAuthorityId,
        Guid industrySectorId,
        Guid projectOwnerId,
        Guid projectGroupId,
        SubProjectType subProjectType = SubProjectType.NotSubProject)
    {
        var project = new DomesticProject
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectCode = projectCode,
            ProjectName = projectName,
            ProjectType = ProjectType.Domestic,
            ManagingAuthorityId = managingAuthorityId,
            IndustrySectorId = industrySectorId,
            ProjectOwnerId = projectOwnerId,
            ProjectGroupId = projectGroupId,
            SubProjectType = subProjectType
        };
        project.RaiseCreatedEvent();
        return project;
    }
}
