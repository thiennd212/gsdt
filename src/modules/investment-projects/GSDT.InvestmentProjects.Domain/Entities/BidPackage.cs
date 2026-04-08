namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Bid package within an investment project — shared by both domestic and ODA projects.
/// Tracks contractor selection, contract award, and links to BidItems and Contracts.
/// </summary>
public sealed class BidPackage : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    /// <summary>Guid ref to contractor selection plan (KHLCNT) this package belongs to.</summary>
    public Guid? ContractorSelectionPlanId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsDesignReview { get; set; }
    public bool IsSupervision { get; set; }

    /// <summary>Guid ref to MasterData: bid selection form (e.g. open tender, limited tender).</summary>
    public Guid BidSelectionFormId { get; set; }

    /// <summary>Guid ref to MasterData: bid selection method (e.g. domestic, international).</summary>
    public Guid BidSelectionMethodId { get; set; }

    /// <summary>Guid ref to MasterData: contract form (lump sum, unit price, cost plus).</summary>
    public Guid ContractFormId { get; set; }

    /// <summary>Guid ref to MasterData: bid sector type (construction, goods, consulting).</summary>
    public Guid BidSectorTypeId { get; set; }

    public int? Duration { get; set; }
    public TimeUnit? DurationUnit { get; set; }
    public int? ContractDuration { get; set; }
    public TimeUnit? ContractDurationUnit { get; set; }

    /// <summary>Guid ref to Organization/MasterData: winning contractor.</summary>
    public Guid? WinningContractorId { get; set; }

    /// <summary>Winning bid price — precision (18,4).</summary>
    public decimal? WinningPrice { get; set; }

    /// <summary>Approved estimated price — precision (18,4).</summary>
    public decimal? EstimatedPrice { get; set; }

    public string? ResultDecisionNumber { get; set; }
    public DateTime? ResultDecisionDate { get; set; }

    /// <summary>Guid ref to Files module: bid result decision document.</summary>
    public Guid? ResultFileId { get; set; }

    public string? Notes { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public InvestmentProject Project { get; set; } = default!;
    public ICollection<BidItem> BidItems { get; set; } = new List<BidItem>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    private BidPackage() { } // EF Core

    /// <summary>Factory method — use instead of object initializer (Id is protected set).</summary>
    public static BidPackage Create(
        Guid tenantId, Guid projectId, string name,
        Guid bidSelectionFormId, Guid bidSelectionMethodId,
        Guid contractFormId, Guid bidSectorTypeId,
        Guid? contractorSelectionPlanId = null,
        bool isDesignReview = false, bool isSupervision = false,
        int? duration = null, TimeUnit? durationUnit = null,
        decimal? estimatedPrice = null, string? notes = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            Name = name,
            ContractorSelectionPlanId = contractorSelectionPlanId,
            IsDesignReview = isDesignReview,
            IsSupervision = isSupervision,
            BidSelectionFormId = bidSelectionFormId,
            BidSelectionMethodId = bidSelectionMethodId,
            ContractFormId = contractFormId,
            BidSectorTypeId = bidSectorTypeId,
            Duration = duration,
            DurationUnit = durationUnit,
            EstimatedPrice = estimatedPrice,
            Notes = notes
        };
}
