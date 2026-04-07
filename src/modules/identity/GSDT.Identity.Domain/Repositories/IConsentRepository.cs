
namespace GSDT.Identity.Domain.Repositories;

/// <summary>Write-side consent repository — read-side uses IReadDbConnection (Dapper).</summary>
public interface IConsentRepository
{
    Task<ConsentRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ConsentRecord>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<ConsentRecord?> GetByUserAndPurposeAsync(Guid userId, string purpose, CancellationToken ct = default);
    Task AddAsync(ConsentRecord record, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
