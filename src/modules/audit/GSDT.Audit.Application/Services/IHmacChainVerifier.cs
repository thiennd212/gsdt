
namespace GSDT.Audit.Application.Services;

/// <summary>
/// Abstracts HMAC chain verification so Application layer has no Infrastructure dependency.
/// Infrastructure provides the implementation via HmacChainService.
/// </summary>
public interface IHmacChainVerifier
{
    Task<ChainVerificationResultDto> VerifyAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
