namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Regulatory violation record for an investment project — shared by domestic and ODA.</summary>
public sealed class ViolationRecord : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    public DateTime ViolationDate { get; set; }

    /// <summary>Guid ref to MasterData: violation type classification.</summary>
    public Guid ViolationTypeId { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>Guid ref to MasterData: enforcement action taken.</summary>
    public Guid ViolationActionId { get; set; }

    /// <summary>Monetary penalty imposed — precision (18,4).</summary>
    public decimal? Penalty { get; set; }

    public string? Notes { get; set; }

    /// <summary>Guid ref to Files module: violation/penalty document.</summary>
    public Guid? FileId { get; set; }

    // Navigation
    public InvestmentProject Project { get; set; } = default!;

    private ViolationRecord() { } // EF Core
}
