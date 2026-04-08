namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>Line-item breakdown within a DesignEstimate.</summary>
public sealed class DesignEstimateItem : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid DesignEstimateId { get; set; }

    /// <summary>Name/description of the estimate line item (max 500).</summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>Scale or specification of the item (max 500, optional).</summary>
    public string? Scale { get; set; }

    /// <summary>Cost of this line item — precision (18,4).</summary>
    public decimal Cost { get; set; }

    /// <summary>Guid ref to Files module: supporting document (optional).</summary>
    public Guid? FileId { get; set; }

    // Navigation
    public DesignEstimate DesignEstimate { get; set; } = default!;

    private DesignEstimateItem() { } // EF Core

    public static DesignEstimateItem Create(
        Guid tenantId, Guid designEstimateId,
        string itemName, decimal cost,
        string? scale = null, Guid? fileId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DesignEstimateId = designEstimateId,
            ItemName = itemName,
            Cost = cost,
            Scale = scale,
            FileId = fileId
        };
}
