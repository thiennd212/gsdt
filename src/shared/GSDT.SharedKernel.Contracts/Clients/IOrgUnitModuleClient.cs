namespace GSDT.SharedKernel.Contracts.Clients;

/// <summary>
/// Cross-module interface for OrgUnit lookups.
/// Monolith: InProcessOrgUnitModuleClient (direct DB query).
/// Microservice: HttpOrgUnitModuleClient (when module extracted — Phase 2+).
/// </summary>
public interface IOrgUnitModuleClient
{
    /// <summary>Max IDs per batch call. Exceeding throws ArgumentException.</summary>
    const int MaxBatchSize = 5000;

    /// <summary>
    /// Batch lookup org unit info by IDs. Returns only found units (missing IDs omitted).
    /// Chunked at 500 IDs internally. Max 5000 IDs per call.
    /// </summary>
    /// <param name="ids">OrgUnit IDs to look up</param>
    /// <param name="tenantId">Optional tenant filter for data isolation [RT-02]</param>
    Task<IReadOnlyDictionary<Guid, OrgUnitInfoDto>> GetOrgUnitsByIdsAsync(
        IEnumerable<Guid> ids, Guid? tenantId = null, CancellationToken ct = default);
}

/// <summary>
/// [RT-12] Class-based DTO for Dapper compatibility.
/// Lightweight org unit info for cross-module display/enrichment.
/// </summary>
public sealed class OrgUnitInfoDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
}
