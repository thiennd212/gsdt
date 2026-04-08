namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Line-item breakdown within a bid package (e.g. individual work items or goods lots).</summary>
public sealed class BidItem : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid BidPackageId { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }

    /// <summary>Estimated unit/total price for this line item — precision (18,4).</summary>
    public decimal? EstimatedPrice { get; set; }

    public string? Notes { get; set; }

    // Navigation
    public BidPackage BidPackage { get; set; } = default!;

    private BidItem() { } // EF Core
}
