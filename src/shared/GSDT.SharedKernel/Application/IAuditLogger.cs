namespace GSDT.SharedKernel.Application;

/// <summary>
/// Minimal audit logging interface — lives in SharedKernel so all behaviors can use it.
/// Implemented by Audit.Infrastructure.Services.AuditService.
/// </summary>
public interface IAuditLogger
{
    Task LogCommandAsync(
        string action,
        string moduleName,
        string resourceType,
        string? resourceId = null,
        CancellationToken cancellationToken = default);
}
