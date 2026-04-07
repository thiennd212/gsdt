
namespace GSDT.Audit.Domain.Repositories;

/// <summary>
/// Append-only repository for AuditLogEntry.
/// No Update/Delete methods — enforcement is in AuditDbContext.
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AuditLogEntry?> GetLatestAsync(CancellationToken cancellationToken = default);
    Task<AuditLogEntry?> GetLatestBeforeSequenceAsync(long sequenceId, CancellationToken cancellationToken = default);
    Task UpdateSignatureAsync(Guid id, string signature, CancellationToken cancellationToken = default);
    /// <summary>Returns all entries for a tenant ordered by SequenceId ascending — used for chain verification.</summary>
    IAsyncEnumerable<AuditLogEntry> GetByTenantOrderedAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
