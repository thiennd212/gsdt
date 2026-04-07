namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>State audit record for an investment project — shared by domestic and ODA.</summary>
public sealed class AuditRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public DateTime AuditDate { get; set; }
    public string AuditAgency { get; set; } = string.Empty;

    /// <summary>Guid ref to MasterData: audit conclusion type (clean, qualified, adverse, etc.).</summary>
    public Guid ConclusionTypeId { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>Guid ref to Files module: audit report document.</summary>
    public Guid? FileId { get; set; }

    // Navigation
    public InvestmentProject Project { get; set; } = default!;

    private AuditRecord() { } // EF Core
}
