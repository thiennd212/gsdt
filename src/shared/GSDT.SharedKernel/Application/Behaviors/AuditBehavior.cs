using FluentResults;
using MediatR;

namespace GSDT.SharedKernel.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior — auto-logs audit entries for all IBaseCommand executions.
/// Only logs on success; logs warning on audit failure but never breaks the main pipeline.
/// </summary>
public sealed class AuditBehavior<TRequest, TResponse>(
    IAuditLogger auditLogger,
    ILogger<AuditBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        // Only audit successful commands
        if (response is IResultBase rb && rb.IsFailed) return response;

        try
        {
            var typeName = typeof(TRequest).Name;
            var moduleName = ExtractModule(typeof(TRequest));
            var action = ExtractAction(typeName);
            var resourceType = ExtractResource(typeName);

            await auditLogger.LogCommandAsync(action, moduleName, resourceType, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // Never let audit logging crash the main pipeline — log warning for observability
            logger.LogWarning(ex, "Audit logging failed for {CommandType}", typeof(TRequest).Name);
        }

        return response;
    }

    private static string ExtractModule(Type type)
    {
        var ns = type.Namespace ?? "";
        var parts = ns.Split('.');
        // GSDT.{Module}.Application.Commands → Module
        return parts.Length >= 2 ? parts[1] : "Unknown";
    }

    private static string ExtractAction(string typeName)
    {
        var name = typeName.Replace("Command", "");
        var verbs = new[] { "Create", "Update", "Delete", "Register", "Lock", "Unlock",
            "Assign", "Sync", "Revoke", "Reset", "Approve", "Reject", "Submit", "Close",
            "Publish", "Mark", "Send", "Ingest", "Delegate", "Report", "Trigger", "Manage" };
        foreach (var v in verbs)
            if (name.StartsWith(v)) return v;
        return name.Length > 20 ? name[..20] : name;
    }

    private static string ExtractResource(string typeName)
    {
        var name = typeName.Replace("Command", "");
        var verbs = new[] { "Create", "Update", "Delete", "Register", "Lock", "Unlock",
            "Assign", "Sync", "Revoke", "Reset", "Approve", "Reject", "Submit", "Close",
            "Publish", "Mark", "Send", "Ingest", "Delegate", "Report", "Trigger", "Manage" };
        foreach (var v in verbs)
            if (name.StartsWith(v)) return name[v.Length..];
        return name;
    }
}
