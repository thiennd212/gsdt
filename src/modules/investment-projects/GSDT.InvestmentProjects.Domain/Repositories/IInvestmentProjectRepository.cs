namespace GSDT.InvestmentProjects.Domain.Repositories;

/// <summary>
/// Repository contract for InvestmentProject aggregate.
/// Implementations live in Infrastructure — domain stays persistence-ignorant.
/// </summary>
public interface IInvestmentProjectRepository
{
    /// <summary>Returns the project (domestic or ODA) by ID, or null if not found / soft-deleted.</summary>
    Task<InvestmentProject?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns a DomesticProject with all child collections eagerly loaded, or null if not found.
    /// Used by GetById query handlers — avoids Application depending on Infrastructure DbContext.
    /// </summary>
    Task<DomesticProject?> GetDomesticByIdWithDetailsAsync(Guid id, Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Returns an OdaProject with all child collections eagerly loaded, or null if not found.
    /// Used by GetById query handlers — avoids Application depending on Infrastructure DbContext.
    /// </summary>
    Task<OdaProject?> GetOdaByIdWithDetailsAsync(Guid id, Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Returns a tracked InvestmentProject with the Locations collection loaded.
    /// Used by AddProjectLocation / DeleteProjectLocation command handlers.
    /// </summary>
    Task<InvestmentProject?> GetByIdWithLocationsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns a tracked DomesticProject with the InvestmentDecisions collection loaded.
    /// Used by AddDomesticDecision / DeleteDomesticDecision command handlers.
    /// </summary>
    Task<DomesticProject?> GetDomesticByIdWithDecisionsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns a tracked InvestmentProject with the BidPackages collection loaded.
    /// Used by AddBidPackage / DeleteBidPackage command handlers.
    /// </summary>
    Task<InvestmentProject?> GetByIdWithBidPackagesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns a tracked InvestmentProject with the Documents collection loaded.
    /// Used by AddProjectDocument / DeleteProjectDocument command handlers.
    /// </summary>
    Task<InvestmentProject?> GetByIdWithDocumentsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns true if an active (non-deleted) project with the given ID exists.</summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns true if ProjectCode already exists for the tenant.
    /// Pass excludeId to ignore the current project (used during update validation).
    /// </summary>
    Task<bool> ProjectCodeExistsAsync(
        string code,
        Guid tenantId,
        Guid? excludeId = null,
        CancellationToken ct = default);

    /// <summary>Stages a new project for insertion — call SaveChanges to persist.</summary>
    void Add(InvestmentProject project);

    /// <summary>Marks a project for soft-delete — call SaveChanges to persist.</summary>
    void Remove(InvestmentProject project);

    /// <summary>Persists all pending changes to the database.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
