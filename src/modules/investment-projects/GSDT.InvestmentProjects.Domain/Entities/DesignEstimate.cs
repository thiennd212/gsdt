namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Design estimate (dự toán thiết kế) for an investment project.
/// FK to InvestmentProject base — available to both PPP and DNNN projects.
/// All monetary fields precision (18,4).
/// </summary>
public sealed class DesignEstimate : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    // Approval decision fields
    public string? ApprovalDecisionNumber { get; set; }
    public DateTime? ApprovalDecisionDate { get; set; }

    /// <summary>Authority that approved the estimate (max 200).</summary>
    public string? ApprovalAuthority { get; set; }

    /// <summary>Signer of the approval decision (max 200).</summary>
    public string? ApprovalSigner { get; set; }

    /// <summary>Summary of approval content (max 2000).</summary>
    public string? ApprovalSummary { get; set; }

    /// <summary>Guid ref to Files module: approval decision document.</summary>
    public Guid? ApprovalFileId { get; set; }

    // Cost breakdown — precision (18,4)
    public decimal EquipmentCost { get; set; }
    public decimal ConstructionCost { get; set; }
    public decimal LandCompensationCost { get; set; }
    public decimal ManagementCost { get; set; }
    public decimal ConsultancyCost { get; set; }
    public decimal ContingencyCost { get; set; }
    public decimal OtherCost { get; set; }
    public decimal TotalEstimate { get; set; }

    public string? Notes { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public InvestmentProject Project { get; set; } = default!;
    public ICollection<DesignEstimateItem> Items { get; set; } = new List<DesignEstimateItem>();

    private DesignEstimate() { } // EF Core

    public static DesignEstimate Create(Guid tenantId, Guid projectId,
        decimal equipmentCost, decimal constructionCost, decimal landCompensationCost,
        decimal managementCost, decimal consultancyCost, decimal contingencyCost,
        decimal otherCost, decimal totalEstimate,
        string? notes = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            EquipmentCost = equipmentCost,
            ConstructionCost = constructionCost,
            LandCompensationCost = landCompensationCost,
            ManagementCost = managementCost,
            ConsultancyCost = consultancyCost,
            ContingencyCost = contingencyCost,
            OtherCost = otherCost,
            TotalEstimate = totalEstimate,
            Notes = notes
        };
}
