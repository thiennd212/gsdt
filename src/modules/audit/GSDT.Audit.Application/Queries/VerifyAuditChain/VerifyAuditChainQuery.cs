using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.VerifyAuditChain;

/// <summary>
/// Query to verify HMAC chain integrity for a tenant's audit log.
/// F-05: compliance audit export — restricted to SystemAdmin.
/// </summary>
public sealed record VerifyAuditChainQuery(Guid TenantId) : IRequest<Result<ChainVerificationResultDto>>;
