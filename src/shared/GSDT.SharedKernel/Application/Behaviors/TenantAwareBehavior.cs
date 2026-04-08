using FluentResults;
using GSDT.SharedKernel.Domain;
using GSDT.SharedKernel.Errors;
using MediatR;

namespace GSDT.SharedKernel.Application.Behaviors;

/// <summary>
/// Validates ITenantContext.TenantId is set before any command handler runs.
/// Registered FIRST in MediatR pipeline (before ValidationBehavior).
/// SystemAdmin role bypasses tenant check for cross-tenant operations.
/// </summary>
public sealed class TenantAwareBehavior<TRequest, TResponse>(ITenantContext tenantContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only enforce on commands (write operations) — queries allowed without tenant
        if (request is not IBaseCommand)
            return await next();

        if (!tenantContext.IsSystemAdmin && !tenantContext.TenantId.HasValue)
        {
            var error = new Error("Tenant context is required for this operation.")
                .WithMetadata("ErrorCode", ErrorCodes.Security.TenantRequired);

            return ResultBehaviorHelper.CreateFail<TResponse>([error]);
        }

        return await next();
    }
}
