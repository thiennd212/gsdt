
namespace GSDT.Audit.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of IAuditService + IAuditLogger.
/// IAuditLogger is used by MediatR AuditBehavior for automatic command logging.
/// </summary>
public sealed class AuditService(
    AuditDbContext db,
    IAuditLogRepository repository,
    IBackgroundAuditJobEnqueuer jobEnqueuer,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUser currentUser) : IAuditService, IAuditLogger
{
    public async Task LogAsync(
        string action,
        string moduleName,
        string resourceType,
        string? resourceId = null,
        string? dataSnapshot = null,
        CancellationToken cancellationToken = default)
    {
        var ctx = httpContextAccessor.HttpContext;
        var ipAddress = ctx?.Connection.RemoteIpAddress?.ToString();
        var correlationId = ctx?.TraceIdentifier;

        var entry = AuditLogEntry.Create(
            currentUser.TenantId,
            currentUser.UserId,
            currentUser.UserName,
            action,
            moduleName,
            resourceType,
            resourceId,
            dataSnapshot,
            ipAddress,
            correlationId);

        await repository.AddAsync(entry, cancellationToken);
        jobEnqueuer.EnqueueHmacChain(entry.Id);
    }

    public async Task LogPersonalDataAccessAsync(
        Guid dataSubjectId,
        string dataCategory,
        string purpose,
        string legalBasis,
        CancellationToken cancellationToken = default)
    {
        var log = PersonalDataProcessingLog.Record(
            currentUser.TenantId ?? Guid.Empty,
            currentUser.UserId,
            dataSubjectId,
            dataCategory,
            purpose,
            legalBasis,
            processingAction: "access");

        db.PersonalDataProcessingLogs.Add(log);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task LogLoginAttemptAsync(
        Guid? userId, string email, string ipAddress, string? userAgent,
        bool success, string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var attempt = LoginAttempt.Record(userId, email, ipAddress, userAgent, success, failureReason);
        db.LoginAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);
    }

    // IAuditLogger — called by MediatR AuditBehavior for automatic command logging
    public Task LogCommandAsync(
        string action, string moduleName, string resourceType,
        string? resourceId = null, CancellationToken cancellationToken = default) =>
        LogAsync(action, moduleName, resourceType, resourceId, null, cancellationToken);
}
