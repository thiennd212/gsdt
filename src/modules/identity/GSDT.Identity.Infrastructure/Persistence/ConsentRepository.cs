
namespace GSDT.Identity.Infrastructure.Persistence;

public sealed class ConsentRepository : IConsentRepository
{
    private readonly IdentityDbContext _db;

    public ConsentRepository(IdentityDbContext db) => _db = db;

    public Task<ConsentRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.ConsentRecords.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<ConsentRecord>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _db.ConsentRecords.Where(c => c.DataSubjectId == userId).OrderByDescending(c => c.CreatedAtUtc).ToListAsync(ct);

    public Task<ConsentRecord?> GetByUserAndPurposeAsync(Guid userId, string purpose, CancellationToken ct = default)
        => _db.ConsentRecords.FirstOrDefaultAsync(c => c.DataSubjectId == userId && c.Purpose == purpose && !c.IsWithdrawn, ct);

    public Task AddAsync(ConsentRecord record, CancellationToken ct = default)
    {
        _db.ConsentRecords.Add(record);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
