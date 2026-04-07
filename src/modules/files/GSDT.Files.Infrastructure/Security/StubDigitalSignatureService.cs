
namespace GSDT.Files.Infrastructure.Security;

/// <summary>
/// ND68 digital signature verification stub.
/// Returns a pass-through result — project-specific implementation required
/// when ND68 signing infrastructure (PKI, CRL endpoint, OCSP) is available.
/// </summary>
public sealed class StubDigitalSignatureService(
    ILogger<StubDigitalSignatureService> logger) : IDigitalSignatureService
{
    private static readonly HashSet<string> SignedDocumentTypes =
        ["application/pdf", "text/xml", "application/xml"];

    public Task<SignatureVerificationResult> VerifyAsync(
        Stream documentStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (!SignedDocumentTypes.Contains(contentType))
            return Task.FromResult(SignatureVerificationResult.NotApplicable());

        logger.LogWarning(
            "ND68 digital signature verification is STUB — no real verification performed. " +
            "ContentType={ContentType}", contentType);

        return Task.FromResult(SignatureVerificationResult.Stub());
    }
}
