using Hangfire;

namespace GSDT.Audit.Infrastructure.Services;

/// <summary>
/// Hangfire-backed implementation of IBackgroundAuditJobEnqueuer.
/// Decouples Application layer from Hangfire — Application only sees the interface.
/// </summary>
public sealed class HangfireAuditJobEnqueuer : IBackgroundAuditJobEnqueuer
{
    public void EnqueueHmacChain(Guid auditLogEntryId) =>
        BackgroundJob.Enqueue<HmacChainService>(svc =>
            svc.ComputeAsync(auditLogEntryId, CancellationToken.None));
}
