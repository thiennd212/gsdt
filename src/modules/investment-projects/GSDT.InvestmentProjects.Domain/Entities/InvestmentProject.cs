namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Base aggregate root for all investment projects (TPT base table: investment.InvestmentProjects).
/// DomesticProject and OdaProject extend this via TPT — each has its own child table.
/// All MasterData references are plain Guids — no FK constraints to masterdata schema.
/// </summary>
public abstract class InvestmentProject : AuditableEntity<Guid>, IAggregateRoot, ITenantScoped
{
    public Guid TenantId { get; set; }

    /// <summary>Unique project code per tenant (max 50 chars).</summary>
    public string ProjectCode { get; set; } = string.Empty;

    /// <summary>Full official project name (max 500 chars).</summary>
    public string ProjectName { get; set; } = string.Empty;

    public ProjectType ProjectType { get; set; }

    /// <summary>Guid ref to MasterData: managing authority org unit.</summary>
    public Guid ManagingAuthorityId { get; set; }

    /// <summary>Guid ref to MasterData: industry/sector classification.</summary>
    public Guid IndustrySectorId { get; set; }

    /// <summary>Guid ref to MasterData or Organization: project owner unit.</summary>
    public Guid ProjectOwnerId { get; set; }

    /// <summary>Optional project management unit (PMU) — separate from owner.</summary>
    public Guid? ProjectManagementUnitId { get; set; }
    public string? PmuDirectorName { get; set; }
    public string? PmuPhone { get; set; }
    public string? PmuEmail { get; set; }

    /// <summary>Free-text implementation period description (e.g. "2024–2026").</summary>
    public string? ImplementationPeriod { get; set; }

    // Policy decision fields
    public string? PolicyDecisionNumber { get; set; }
    public DateTime? PolicyDecisionDate { get; set; }
    public string? PolicyDecisionAuthority { get; set; }
    public string? PolicyDecisionPerson { get; set; }

    /// <summary>Guid ref to Files module: uploaded policy decision document.</summary>
    public Guid? PolicyDecisionFileId { get; set; }

    /// <summary>EF concurrency token — optimistic locking.</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation properties
    public ICollection<ProjectLocation> Locations { get; set; } = new List<ProjectLocation>();
    public ICollection<BidPackage> BidPackages { get; set; } = new List<BidPackage>();
    public ICollection<InspectionRecord> InspectionRecords { get; set; } = new List<InspectionRecord>();
    public ICollection<EvaluationRecord> EvaluationRecords { get; set; } = new List<EvaluationRecord>();
    public ICollection<AuditRecord> AuditRecords { get; set; } = new List<AuditRecord>();
    public ICollection<ViolationRecord> ViolationRecords { get; set; } = new List<ViolationRecord>();
    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
    public OperationInfo? OperationInfo { get; set; }

    /// <summary>Design estimate records — available on both PPP and DNNN projects.</summary>
    public ICollection<DesignEstimate> DesignEstimates { get; set; } = new List<DesignEstimate>();

    /// <summary>Investor selection record — 1-to-1, shared PK (ProjectId).</summary>
    public InvestorSelection? InvestorSelection { get; set; }

    protected InvestmentProject() { } // EF Core

    /// <summary>Raises ProjectCreatedEvent — call from concrete factory methods in subclasses.</summary>
    protected void RaiseCreatedEvent()
    {
        AddDomainEvent(new Events.ProjectCreatedEvent(Id, TenantId, ProjectType));
    }

    /// <summary>Soft-deletes the project and raises ProjectDeletedEvent.</summary>
    public void SoftDelete()
    {
        MarkDeleted();
        AddDomainEvent(new Events.ProjectDeletedEvent(Id, TenantId));
    }

    /// <summary>Clears collected domain events after outbox drain — required by IAggregateRoot.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
