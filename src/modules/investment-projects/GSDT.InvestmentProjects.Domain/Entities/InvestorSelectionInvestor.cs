namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Junction table linking an InvestorSelection to one or more investors.
/// Composite PK: (InvestorSelectionId, InvestorId).
/// InvestorId is a plain Guid reference — no FK to Investor entity (cross-module boundary).
/// No audit trail — pure join record.
/// </summary>
public sealed class InvestorSelectionInvestor
{
    /// <summary>FK → InvestorSelection.ProjectId (composite PK part 1).</summary>
    public Guid InvestorSelectionId { get; set; }

    /// <summary>Guid ref to Investor entity in another module (composite PK part 2).</summary>
    public Guid InvestorId { get; set; }

    /// <summary>Display sort order within the selection list.</summary>
    public int SortOrder { get; set; }

    private InvestorSelectionInvestor() { } // EF Core

    public static InvestorSelectionInvestor Create(Guid investorSelectionId, Guid investorId, int sortOrder = 0)
        => new()
        {
            InvestorSelectionId = investorSelectionId,
            InvestorId = investorId,
            SortOrder = sortOrder
        };
}
