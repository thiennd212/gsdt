
namespace GSDT.Identity.Domain.Repositories;

/// <summary>Write-side delegation repository — read-side uses IReadDbConnection (Dapper).</summary>
public interface IDelegationRepository
{
    Task<UserDelegation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasActiveOverlapAsync(Guid delegatorId, Guid delegateId, DateTime validFrom, DateTime validTo, CancellationToken ct = default);
    Task AddAsync(UserDelegation delegation, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
