namespace GSDT.SharedKernel.Contracts.Clients;

/// <summary>
/// Cross-module interface for signature access.
/// Monolith: InProcessSignatureModuleClient (direct DB query, zero overhead).
/// Microservice: GrpcSignatureModuleClient (when module extracted).
/// </summary>
public interface ISignatureModuleClient
{
    Task<Guid> RequestSignatureAsync(SignatureRequestInput request, CancellationToken ct = default);
    Task<SignatureVerificationResult> VerifyAsync(Guid documentId, Guid tenantId, CancellationToken ct = default);
    Task<SignatureStatusInfo?> GetStatusAsync(Guid requestId, Guid tenantId, CancellationToken ct = default);
}

public sealed record SignatureRequestInput(Guid DocumentId, Guid TenantId, Guid RequestedBy, IReadOnlyList<Guid> SignerUserIds);
public sealed record SignatureVerificationResult(bool IsValid, string? Reason, DateTime? ValidatedAt);
public sealed record SignatureStatusInfo(Guid RequestId, string Status, int SignedCount, int TotalSigners);
