
namespace GSDT.Audit.Domain.Repositories;

/// <summary>Repository for RtbfRequest — Law 91/2025 RTBF tracking.</summary>
public interface IRtbfRequestRepository
{
    Task AddAsync(RtbfRequest request, CancellationToken cancellationToken = default);
    Task<RtbfRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
