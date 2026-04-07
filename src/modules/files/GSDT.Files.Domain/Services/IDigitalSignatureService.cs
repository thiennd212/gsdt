namespace GSDT.Files.Domain.Services;

/// <summary>
/// ND68 digital signature verification interface — stub only in this phase.
/// Project-specific implementation added when ND68 signing infrastructure is available.
/// Supports PDF (iTextSharp) and XML (XmlDsig) documents.
/// </summary>
public interface IDigitalSignatureService
{
    /// <summary>Verify digital signatures in a PDF or XML document.</summary>
    Task<SignatureVerificationResult> VerifyAsync(Stream documentStream, string contentType, CancellationToken cancellationToken = default);
}

public sealed record SignatureVerificationResult(
    bool IsValid,
    string? SignerDistinguishedName,
    string? CertificateSerial,
    DateTimeOffset? SignedAt,
    string? FailureReason = null)
{
    public static SignatureVerificationResult NotApplicable() =>
        new(true, null, null, null);

    public static SignatureVerificationResult Stub() =>
        new(true, "STUB - ND68 not implemented", "N/A", DateTimeOffset.UtcNow);
}
