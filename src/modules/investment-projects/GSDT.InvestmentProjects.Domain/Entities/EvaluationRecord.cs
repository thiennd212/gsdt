namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Mid-term or final evaluation record for an investment project — shared by domestic and ODA.</summary>
public sealed class EvaluationRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public DateTime EvaluationDate { get; set; }

    /// <summary>Guid ref to MasterData: evaluation type (initial, mid-term, final, impact).</summary>
    public Guid EvaluationTypeId { get; set; }

    public string Content { get; set; } = string.Empty;
    public string? Result { get; set; }

    /// <summary>Guid ref to Files module: evaluation report document.</summary>
    public Guid? FileId { get; set; }

    // Navigation
    public InvestmentProject Project { get; set; } = default!;

    private EvaluationRecord() { } // EF Core
}
