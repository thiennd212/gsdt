
namespace GSDT.Audit.Infrastructure.Persistence;

/// <summary>Append-only EF repository for AuditLogEntry.</summary>
public sealed class AuditLogRepository(AuditDbContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        db.AuditLogEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.AuditLogEntries.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<AuditLogEntry?> GetLatestAsync(CancellationToken cancellationToken = default) =>
        db.AuditLogEntries.AsNoTracking()
            .OrderByDescending(e => e.SequenceId)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<AuditLogEntry?> GetLatestBeforeSequenceAsync(long sequenceId, CancellationToken cancellationToken = default) =>
        db.AuditLogEntries.AsNoTracking()
            .Where(e => e.SequenceId < sequenceId)
            .OrderByDescending(e => e.SequenceId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task UpdateSignatureAsync(Guid id, string signature, CancellationToken cancellationToken = default)
    {
        // Direct update via ExecuteUpdateAsync — no change tracking, avoids append-only guard
        await db.AuditLogEntries
            .Where(e => e.Id == id && e.HmacSignature == string.Empty)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.HmacSignature, signature), cancellationToken);
    }

    /// <summary>Streams all entries for a tenant in SequenceId order — used for chain verification.</summary>
    public IAsyncEnumerable<AuditLogEntry> GetByTenantOrderedAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        db.AuditLogEntries.AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .OrderBy(e => e.SequenceId)
            .AsAsyncEnumerable();
}
