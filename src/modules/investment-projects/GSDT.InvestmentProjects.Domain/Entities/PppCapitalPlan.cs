namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>State capital allocation plan decision for a PPP project.</summary>
public sealed class PppCapitalPlan : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    /// <summary>Type of capital decision (max 100).</summary>
    public string DecisionType { get; set; } = string.Empty;

    public string DecisionNumber { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }

    /// <summary>State capital amount per this decision — precision (18,4).</summary>
    public decimal StateCapitalByDecision { get; set; }

    /// <summary>Guid ref to Files module: attached plan document.</summary>
    public Guid? FileId { get; set; }

    public string? Notes { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public PppProject Project { get; set; } = default!;

    private PppCapitalPlan() { } // EF Core

    public static PppCapitalPlan Create(
        Guid tenantId, Guid projectId,
        string decisionType, string decisionNumber, DateTime decisionDate,
        decimal stateCapitalByDecision,
        Guid? fileId = null, string? notes = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            DecisionType = decisionType,
            DecisionNumber = decisionNumber,
            DecisionDate = decisionDate,
            StateCapitalByDecision = stateCapitalByDecision,
            FileId = fileId,
            Notes = notes
        };
}
