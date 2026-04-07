
namespace GSDT.Identity.Domain.Repositories;

/// <summary>Access review repository — write side for QĐ742 periodic access review workflow.</summary>
public interface IAccessReviewRepository
{
    Task<AccessReviewRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns pending records (Decision == null) with optional tenant filter.</summary>
    Task<IReadOnlyList<AccessReviewRecord>> ListPendingAsync(
        Guid? tenantId, int page, int pageSize, CancellationToken ct = default);

    Task UpdateAsync(AccessReviewRecord record, CancellationToken ct = default);
}
