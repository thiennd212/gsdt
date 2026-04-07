
namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// MFA endpoints for TOTP setup/verify and email OTP dispatch.
/// Accessible to any authenticated user (not just admins).
/// </summary>
[Route("api/v1/mfa")]
[Authorize]
public sealed class MfaController(IMfaService mfaService) : ApiControllerBase
{
    /// <summary>
    /// GET /api/v1/mfa/setup
    /// Returns an otpauth:// URI for TOTP enrollment — client renders as QR code.
    /// </summary>
    [HttpGet("setup")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Setup()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var uri = await mfaService.GenerateTotpSetupAsync(userId);
        return Ok(new { otpauthUri = uri });
    }

    /// <summary>
    /// POST /api/v1/mfa/verify
    /// Validates a TOTP code from the authenticator app.
    /// Body: { "code": "123456" }
    /// </summary>
    [HttpPost("verify")]
    [EnableRateLimiting("mfa-verify")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Verify([FromBody] VerifyTotpRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var valid = await mfaService.ValidateTotpAsync(userId, request.Code);
        if (!valid)
            return BadRequest(new { error = "Invalid or expired TOTP code." });

        return Ok(new { verified = true });
    }

    /// <summary>
    /// POST /api/v1/mfa/send-otp
    /// Enqueues a Hangfire job to send a 6-digit email OTP to the current user.
    /// </summary>
    [HttpPost("send-otp")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(202)]
    public async Task<IActionResult> SendOtp()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        await mfaService.SendEmailOtpAsync(userId);
        return Accepted();
    }

    // --- helpers ---

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

/// <summary>Request body for TOTP verification.</summary>
public sealed record VerifyTotpRequest(string Code);
