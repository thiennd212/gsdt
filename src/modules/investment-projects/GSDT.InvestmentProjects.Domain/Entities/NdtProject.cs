namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Private domestic investor (NĐT trong nước) project (TPT child table: investment.NdtProjects).
/// Clone of DNNN minus DesignEstimate + InvestorSelection. Capital structure: CSH/ODA/TCTD.
/// </summary>
public sealed class NdtProject : InvestmentProject
{
    public SubProjectType SubProjectType { get; set; }

    /// <summary>Guid ref to MasterData: project group classification.</summary>
    public Guid ProjectGroupId { get; set; }

    /// <summary>Guid ref to MasterData: current project status.</summary>
    public Guid StatusId { get; set; }

    /// <summary>Guid ref to GovernmentAgency catalog: competent authority (CQCQ).</summary>
    public Guid? CompetentAuthorityId { get; set; }

    /// <summary>Investor name free-text (max 500).</summary>
    public string? InvestorName { get; set; }

    /// <summary>State ownership ratio as percentage — precision (5,2).</summary>
    public decimal? StateOwnershipRatio { get; set; }

    /// <summary>Project objectives and scope description (max 2000).</summary>
    public string? Objective { get; set; }

    // Preliminary capital estimates — same DNNN structure: CSH + ODA + TCTD
    public decimal PrelimTotalInvestment { get; set; }
    public decimal PrelimEquityCapital { get; set; }
    public decimal PrelimOdaLoanCapital { get; set; }
    public decimal PrelimCreditLoanCapital { get; set; }

    /// <summary>Total project land area in hectares — precision (18,4).</summary>
    public decimal? AreaHectares { get; set; }

    /// <summary>Project capacity description (max 500).</summary>
    public string? Capacity { get; set; }

    /// <summary>Main construction items description (max 2000).</summary>
    public string? MainItems { get; set; }

    /// <summary>Implementation timeline description (max 200).</summary>
    public string? ImplementationTimeline { get; set; }

    /// <summary>Progress description (max 1000).</summary>
    public string? ProgressDescription { get; set; }

    // Suspension/stop fields
    public string? StopContent { get; set; }
    public string? StopDecisionNumber { get; set; }
    public DateTime? StopDecisionDate { get; set; }

    /// <summary>Guid ref to Files module: suspension decision document.</summary>
    public Guid? StopFileId { get; set; }

    // Navigation properties — NĐT children (NO DesignEstimate, NO InvestorSelection)
    public ICollection<NdtInvestmentDecision> InvestmentDecisions { get; set; } = new List<NdtInvestmentDecision>();
    // RegistrationCertificates nav is inherited from InvestmentProject base

    private NdtProject() { } // EF Core

    /// <summary>Factory method — raises ProjectCreatedEvent.</summary>
    public static NdtProject Create(
        Guid tenantId,
        string projectCode,
        string projectName,
        Guid managingAuthorityId,
        Guid industrySectorId,
        Guid projectOwnerId,
        Guid projectGroupId,
        SubProjectType subProjectType = SubProjectType.NotSubProject)
    {
        var project = new NdtProject
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectCode = projectCode,
            ProjectName = projectName,
            ProjectType = ProjectType.Ndt,
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
