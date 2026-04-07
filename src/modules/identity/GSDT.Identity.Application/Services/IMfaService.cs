namespace GSDT.Identity.Application.Services;

/// <summary>MFA operations — TOTP setup/verify and Email OTP dispatch.</summary>
public interface IMfaService
{
    Task<string> GenerateTotpSetupAsync(Guid userId);
    Task<bool> ValidateTotpAsync(Guid userId, string code);
    Task SendEmailOtpAsync(Guid userId);
}
