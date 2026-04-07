
namespace GSDT.Integration.Domain.Repositories;

public interface IMessageLogRepository
{
    Task<MessageLog?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(MessageLog log, CancellationToken ct);
    Task UpdateAsync(MessageLog log, CancellationToken ct);
}
