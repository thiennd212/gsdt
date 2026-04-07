namespace GSDT.Audit.Application.Services;

/// <summary>
/// Abstracts Hangfire enqueueing so Application layer has no Hangfire dependency.
/// Infrastructure provides the implementation.
/// </summary>
public interface IBackgroundAuditJobEnqueuer
{
    /// <summary>Enqueue async HMAC chain computation for the given audit entry.</summary>
    void EnqueueHmacChain(Guid auditLogEntryId);
}
