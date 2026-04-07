
namespace GSDT.Audit.Domain.Repositories;

/// <summary>Repository for AiOutputReview — supports create and status update via Submit().</summary>
public interface IAiOutputReviewRepository
{
    Task AddAsync(AiOutputReview review, CancellationToken cancellationToken = default);
    Task<AiOutputReview?> GetByPromptTraceIdAsync(Guid promptTraceId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
