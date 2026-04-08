namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Inspection visit record for an investment project — shared by domestic and ODA.</summary>
public sealed class InspectionRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public DateTime InspectionDate { get; set; }
    public string InspectionAgency { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Conclusion { get; set; }

    /// <summary>Guid ref to Files module: inspection report document.</summary>
    public Guid? FileId { get; set; }

    // Navigation
    public InvestmentProject Project { get; set; } = default!;

    private InspectionRecord() { } // EF Core
}
