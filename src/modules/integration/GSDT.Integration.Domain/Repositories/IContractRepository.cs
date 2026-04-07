
namespace GSDT.Integration.Domain.Repositories;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Contract contract, CancellationToken ct);
    Task UpdateAsync(Contract contract, CancellationToken ct);
}
