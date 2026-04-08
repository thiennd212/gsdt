namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Investor selection record for a project — 1-to-1 with InvestmentProject base.
/// ProjectId is both PK and FK (shared primary key pattern).
/// Linked to base InvestmentProject (not PppProject) to allow reuse by DNNN projects.
/// </summary>
public sealed class InvestorSelection : AuditableEntity<Guid>, ITenantScoped
{
    /// <summary>Same value as parent InvestmentProject.Id — shared PK/FK.</summary>
    public Guid ProjectId { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Selection method description (max 200).</summary>
    public string? SelectionMethod { get; set; }

    /// <summary>Decision number approving the selected investor (max 100).</summary>
    public string? SelectionDecisionNumber { get; set; }

    public DateTime? SelectionDecisionDate { get; set; }

    /// <summary>Guid ref to Files module: selection decision document.</summary>
    public Guid? SelectionFileId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation
    public InvestmentProject Project { get; set; } = default!;
    public ICollection<InvestorSelectionInvestor> Investors { get; set; } = new List<InvestorSelectionInvestor>();

    private InvestorSelection() { } // EF Core

    public static InvestorSelection Create(Guid projectId, Guid tenantId,
        string? selectionMethod = null, string? selectionDecisionNumber = null,
        DateTime? selectionDecisionDate = null, Guid? selectionFileId = null)
        => new()
        {
            ProjectId = projectId,
            TenantId = tenantId,
            SelectionMethod = selectionMethod,
            SelectionDecisionNumber = selectionDecisionNumber,
            SelectionDecisionDate = selectionDecisionDate,
            SelectionFileId = selectionFileId
        };
}
