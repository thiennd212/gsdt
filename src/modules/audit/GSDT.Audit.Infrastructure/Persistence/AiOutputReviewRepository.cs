
namespace GSDT.Audit.Infrastructure.Persistence;

public sealed class AiOutputReviewRepository(AuditDbContext db) : IAiOutputReviewRepository
{
    public async Task AddAsync(AiOutputReview review, CancellationToken cancellationToken = default)
    {
        db.AiOutputReviews.Add(review);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<AiOutputReview?> GetByPromptTraceIdAsync(Guid promptTraceId, CancellationToken cancellationToken = default) =>
        db.AiOutputReviews.FirstOrDefaultAsync(r => r.PromptTraceId == promptTraceId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
