
namespace GSDT.Audit.Infrastructure.Persistence;

public sealed class AiPromptTraceRepository(AuditDbContext db) : IAiPromptTraceRepository
{
    public async Task AddAsync(AiPromptTrace trace, CancellationToken cancellationToken = default)
    {
        db.AiPromptTraces.Add(trace);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<AiPromptTrace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.AiPromptTraces.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}
