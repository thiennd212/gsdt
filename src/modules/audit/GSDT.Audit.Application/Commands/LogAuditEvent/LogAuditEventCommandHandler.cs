using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.LogAuditEvent;

/// <summary>
/// Inserts an audit entry immediately (synchronous write).
/// HMAC chain computation is enqueued as a Hangfire job post-insert.
/// </summary>
public sealed class LogAuditEventCommandHandler(
    IAuditLogRepository repository,
    IBackgroundAuditJobEnqueuer jobEnqueuer) : IRequestHandler<LogAuditEventCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        LogAuditEventCommand request,
        CancellationToken cancellationToken)
    {
        var entry = AuditLogEntry.Create(
            request.TenantId,
            request.UserId,
            request.UserName,
            request.Action,
            request.ModuleName,
            request.ResourceType,
            request.ResourceId,
            request.DataSnapshot,
            request.IpAddress,
            request.CorrelationId);

        await repository.AddAsync(entry, cancellationToken);

        // Async HMAC chain — does NOT block the write path
        jobEnqueuer.EnqueueHmacChain(entry.Id);

        return Result.Ok(entry.Id);
    }
}
