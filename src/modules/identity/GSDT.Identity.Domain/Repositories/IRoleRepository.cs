
namespace GSDT.Identity.Domain.Repositories;

/// <summary>Write-side role repository.</summary>
public interface IRoleRepository
{
    Task<ApplicationRole?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApplicationRole?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationRole>> ListAllAsync(CancellationToken ct = default);
}
