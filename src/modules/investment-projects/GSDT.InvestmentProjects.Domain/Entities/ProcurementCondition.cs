namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Donor-imposed procurement conditions for an ODA project — 1-to-1 with OdaProject.
/// ProjectId is both PK and FK (shared primary key pattern).
/// </summary>
public sealed class ProcurementCondition : AuditableEntity<Guid>, ITenantScoped
{
    /// <summary>Same value as the parent OdaProject.Id — shared PK/FK.</summary>
    public Guid ProjectId { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Whether the procurement is bound by donor conditions.</summary>
    public bool IsBound { get; set; }

    public string? Summary { get; set; }

    /// <summary>Whether donor approval is required before procurement actions.</summary>
    public bool DonorApprovalRequired { get; set; }

    public string? SpecialConditions { get; set; }

    // Navigation
    public OdaProject Project { get; set; } = default!;

    private ProcurementCondition() { } // EF Core
}
