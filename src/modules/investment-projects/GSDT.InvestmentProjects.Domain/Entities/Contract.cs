namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Contract signed under a bid package — tracks contractor, value, and attached document.</summary>
public sealed class Contract : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid BidPackageId { get; set; }

    public string ContractNumber { get; set; } = string.Empty;
    public DateTime ContractDate { get; set; }

    /// <summary>Guid ref to Organization/MasterData: contracted party.</summary>
    public Guid ContractorId { get; set; }

    /// <summary>Total contract value — precision (18,4).</summary>
    public decimal ContractValue { get; set; }

    /// <summary>Guid ref to MasterData: contract form.</summary>
    public Guid ContractFormId { get; set; }

    public string? Notes { get; set; }

    /// <summary>Guid ref to Files module: signed contract document.</summary>
    public Guid? FileId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public BidPackage BidPackage { get; set; } = default!;

    private Contract() { } // EF Core
}
