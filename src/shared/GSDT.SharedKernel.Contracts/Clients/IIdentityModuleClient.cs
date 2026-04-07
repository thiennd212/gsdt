namespace GSDT.SharedKernel.Contracts.Clients;

/// <summary>
/// Cross-module interface for Identity lookups.
/// Monolith: InProcessIdentityModuleClient (direct DB query, zero overhead).
/// Microservice: GrpcIdentityModuleClient (when module extracted — Phase 2+).
/// </summary>
public interface IIdentityModuleClient
{
    /// <summary>Max IDs per batch call. Exceeding throws ArgumentException.</summary>
    const int MaxBatchSize = 5000;

    /// <summary>
    /// Resolve user by email. Returns null if not found.
    /// [RT-05/VAL-01] Modified from UserLookupResult to UserInfoDto for richer data.
    /// </summary>
    /// <param name="email">Email to search</param>
    /// <param name="tenantId">Optional tenant filter for data isolation</param>
    Task<UserInfoDto?> FindByEmailAsync(string email, Guid? tenantId = null, CancellationToken ct = default);

    /// <summary>
    /// Batch lookup user info by IDs. Returns only found users (missing IDs omitted).
    /// Chunked at 500 IDs internally. Max 5000 IDs per call.
    /// </summary>
    /// <param name="userIds">User IDs to look up</param>
    /// <param name="tenantId">Optional tenant filter for data isolation [RT-02]</param>
    Task<IReadOnlyDictionary<Guid, UserInfoDto>> GetUserInfoByIdsAsync(
        IEnumerable<Guid> userIds, Guid? tenantId = null, CancellationToken ct = default);
}

/// <summary>
/// [RT-12] Class-based DTO — positional records fail Dapper materialization with nullable params.
/// Used for cross-module user info enrichment.
/// </summary>
public sealed class UserInfoDto
{
    public Guid UserId { get; init; }
    public string? Email { get; init; }
    public string? FullName { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? PrimaryOrgUnitId { get; init; }
}
