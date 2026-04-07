namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Bank providing financial services (disbursement, custody) for an ODA project.</summary>
public sealed class ServiceBank : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    /// <summary>Guid ref to MasterData/Organization: the bank entity.</summary>
    public Guid BankId { get; set; }

    /// <summary>Role description (e.g. "Disbursement bank", "Custody bank" — max 200 chars).</summary>
    public string Role { get; set; } = string.Empty;

    public string? Notes { get; set; }

    // Navigation
    public OdaProject Project { get; set; } = default!;

    private ServiceBank() { } // EF Core
}
