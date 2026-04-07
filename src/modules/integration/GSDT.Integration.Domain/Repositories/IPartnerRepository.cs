
namespace GSDT.Integration.Domain.Repositories;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Partner partner, CancellationToken ct);
    Task UpdateAsync(Partner partner, CancellationToken ct);
}
