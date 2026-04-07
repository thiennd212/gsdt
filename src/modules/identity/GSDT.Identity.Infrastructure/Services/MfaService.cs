using Hangfire;
using OtpNet;

namespace GSDT.Identity.Infrastructure.Services;

/// <summary>
/// MFA service supporting TOTP (OtpNet, 30s window) and Email OTP (dispatched via Hangfire).
/// TOTP secrets stored in ASP.NET Identity authenticator key store.
/// </summary>
public sealed class MfaService : IMfaService
{
    private const string TotpIssuer = "GSDT";
    private const int TotpWindowSize = 1; // ±1 step tolerance (30s each side)

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBackgroundJobClient _jobs;
    private readonly ILogger<MfaService> _logger;

    public MfaService(
        UserManager<ApplicationUser> userManager,
        IBackgroundJobClient jobs,
        ILogger<MfaService> logger)
    {
        _userManager = userManager;
        _jobs = jobs;
        _logger = logger;
    }

    /// <summary>
    /// Generates or resets TOTP secret for user.
    /// Returns an otpauth:// URI suitable for QR code rendering by the client.
    /// </summary>
    public async Task<string> GenerateTotpSetupAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"User {userId} not found.");

        // Reset the authenticator key — generates a new base32 secret
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var rawKey = await _userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(rawKey))
            throw new InvalidOperationException("Failed to generate authenticator key.");

        // Build standard otpauth URI
        var encodedIssuer = Uri.EscapeDataString(TotpIssuer);
        var encodedAccount = Uri.EscapeDataString(user.Email ?? user.UserName ?? userId.ToString());
        var uri = $"otpauth://totp/{encodedIssuer}:{encodedAccount}?secret={rawKey}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";

        _logger.LogInformation("TOTP setup generated for user {UserId}", userId);
        return uri;
    }

    /// <summary>
    /// Validates a TOTP code against the user's stored authenticator key.
    /// Accepts ±1 time step (30s tolerance).
    /// </summary>
    public async Task<bool> ValidateTotpAsync(Guid userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var rawKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(rawKey)) return false;

        var keyBytes = Base32Encoding.ToBytes(rawKey);
        var totp = new Totp(keyBytes);

        // VerifyTotp with window validates current step ± TotpWindowSize steps
        var valid = totp.VerifyTotp(
            totp: code,
            timeStepMatched: out _,
            window: new VerificationWindow(TotpWindowSize, TotpWindowSize));

        _logger.LogInformation("TOTP validation for user {UserId}: {Result}", userId, valid ? "success" : "failed");
        return valid;
    }

    /// <summary>
    /// Enqueues a Hangfire job to send a 6-digit OTP to the user's registered email.
    /// The job itself generates the OTP and sends the email — keeping this method fast.
    /// </summary>
    public async Task SendEmailOtpAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            _logger.LogWarning("SendEmailOtp called for non-existent user {UserId}", userId);
            return;
        }

        var email = user.Email;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("User {UserId} has no email — cannot send OTP", userId);
            return;
        }

        // Enqueue to Hangfire — job sends OTP email asynchronously
        _jobs.Enqueue<EmailOtpJob>(job => job.SendAsync(userId, email, CancellationToken.None));

        _logger.LogInformation("Email OTP job enqueued for user {UserId}", userId);
    }
}

/// <summary>
/// Hangfire job: generates OTP token via Identity token provider and sends via email.
/// Kept in same file to avoid unnecessary file proliferation (YAGNI).
/// </summary>
public sealed class EmailOtpJob
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EmailOtpJob> _logger;

    public EmailOtpJob(UserManager<ApplicationUser> userManager, ILogger<EmailOtpJob> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SendAsync(Guid userId, string email, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return;

        // Generate a short-lived token using the "Email" purpose
        var token = await _userManager.GenerateUserTokenAsync(
            user, TokenOptions.DefaultEmailProvider, "EmailOtp");

        // In production: inject IEmailSender and send the token (Phase 07b)
        // Token intentionally omitted from log to prevent credential exposure via log sinks
        _logger.LogDebug("Email OTP generated for {Email} (token omitted from log)", email);
    }
}
