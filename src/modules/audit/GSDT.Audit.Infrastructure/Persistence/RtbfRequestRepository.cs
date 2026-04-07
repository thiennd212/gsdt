
namespace GSDT.Audit.Infrastructure.Persistence;

public sealed class RtbfRequestRepository(AuditDbContext db) : IRtbfRequestRepository
{
    public async Task AddAsync(RtbfRequest request, CancellationToken cancellationToken = default)
    {
        db.RtbfRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<RtbfRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.RtbfRequests.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
