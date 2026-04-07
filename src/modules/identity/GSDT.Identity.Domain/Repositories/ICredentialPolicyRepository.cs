
namespace GSDT.Identity.Domain.Repositories;

/// <summary>Write-side repository for CredentialPolicy — read-side uses IReadDbConnection (Dapper).</summary>
public interface ICredentialPolicyRepository
{
    Task<CredentialPolicy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CredentialPolicy?> GetDefaultForTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<CredentialPolicy>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(CredentialPolicy entity, CancellationToken ct = default);
    Task UpdateAsync(CredentialPolicy entity, CancellationToken ct = default);
    Task DeleteAsync(CredentialPolicy entity, CancellationToken ct = default);
}
