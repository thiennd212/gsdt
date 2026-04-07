using System.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GSDT.AuthServer.Controllers;

/// <summary>
/// Handles OpenIddict authorization + token endpoints.
/// Authorization Code + PKCE for SPA, Password (ROPC) for dev tooling.
/// </summary>
public class AuthorizationController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAuditService auditService) : Controller
{
    // GET /connect/authorize — PKCE authorization code flow
    [HttpGet("/connect/authorize"), HttpPost("/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            // Not logged in → redirect to login page, preserving the OIDC query string
            var returnUrl = Request.PathBase + Request.Path + Request.QueryString;
            return LocalRedirect($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var user = await userManager.GetUserAsync(result.Principal!);
        if (user is null) return Forbid();

        // Build identity with claims OpenIddict needs (including roles)
        var identity = await CreateIdentity(user);

        // Set scopes from the OIDC request
        var request = HttpContext.GetOpenIddictServerRequest()!;
        identity.SetScopes(request.GetScopes());
        // Include API resource as audience — ensures introspection returns all claims to API
        identity.SetResources("gsdt-api-resource");

        // OpenIddict generates the auth code + handles PKCE validation
        return SignIn(new ClaimsPrincipal(identity),
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // POST /connect/token — handles auth code exchange + ROPC
    [HttpPost("/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()!;

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // OpenIddict already validated the auth code / refresh token
            var principal = (await HttpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal!;

            var userId = principal.GetClaim(Claims.Subject);
            var user = userId is not null ? await userManager.FindByIdAsync(userId) : null;
            if (user is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Refresh claims from current user data (including roles)
            var identity = await CreateIdentity(user);
            identity.SetScopes(principal.GetScopes());
            identity.SetResources("gsdt-api-resource");

            return SignIn(new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsPasswordGrantType())
        {
            // ROPC — dev only (gated by AllowPasswordFlow in Program.cs)
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var ua = Request.Headers.UserAgent.FirstOrDefault();
            var username = request.Username ?? "";

            var user = await userManager.FindByEmailAsync(username)
                       ?? await userManager.FindByNameAsync(username);

            if (user is null)
            {
                await auditService.LogLoginAttemptAsync(
                    null, username, ip, ua, false, "ROPC: user not found");
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Use SignInManager to trigger lockout counting (QĐ742 §5.2 compliance)
            var signInResult = await signInManager.CheckPasswordSignInAsync(
                user, request.Password!, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                await auditService.LogLoginAttemptAsync(
                    user.Id, username, ip, ua, false, "ROPC: account locked out");
                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                        OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "Account is locked out due to too many failed login attempts. Please try again later."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (!signInResult.Succeeded)
            {
                await auditService.LogLoginAttemptAsync(
                    user.Id, username, ip, ua, false, "ROPC: invalid credentials");
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Block ROPC for MFA-enabled users — MFA requires interactive browser flow
            if (signInResult.RequiresTwoFactor)
            {
                await auditService.LogLoginAttemptAsync(
                    user.Id, username, ip, ua, false, "ROPC: MFA-enabled account");
                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                        OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "MFA-enabled accounts cannot use password grant. Use authorization code flow."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            await auditService.LogLoginAttemptAsync(user.Id, username, ip, ua, true, "ROPC");

            var identity = await CreateIdentity(user);
            identity.SetScopes(request.GetScopes());
            identity.SetResources("gsdt-api-resource");

            return SignIn(new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest(new { error = "unsupported_grant_type" });
    }

    // GET /account/login — render login form
    [HttpGet("/account/login")]
    public async Task<IActionResult> Login(string? returnUrl, string? error)
    {
        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
        ViewData["ExternalSchemes"] = schemes.ToList();
        if (!string.IsNullOrEmpty(error))
            ModelState.AddModelError("", error switch
            {
                "external_auth_failed" => "Xác thực SSO thất bại. Vui lòng thử lại.",
                "no_linked_account" => "Tài khoản SSO chưa được liên kết. Vui lòng liên hệ quản trị viên.",
                "account_deactivated" => "Tài khoản đã bị vô hiệu hóa.",
                "domain_not_allowed" => "Email của bạn không nằm trong danh sách domain được phép.",
                "provision_failed" => "Không thể tạo tài khoản tự động. Vui lòng liên hệ quản trị viên.",
                "jit_disabled" => "Đăng nhập SSO tự động chưa được bật cho nhà cung cấp này.",
                "rate_limited" => "Hệ thống đang quá tải. Vui lòng thử lại sau.",
                _ => "Đã xảy ra lỗi. Vui lòng thử lại."
            });
        return View("~/Views/Account/Login.cshtml", new LoginViewModel { ReturnUrl = returnUrl ?? "/" });
    }

    // POST /account/login — validate credentials, sign in with Identity cookie
    [HttpPost("/account/login")]
    public async Task<IActionResult> LoginPost(LoginViewModel model)
    {
        // Pass external schemes to view for SSO buttons on validation error
        var externalSchemes = await signInManager.GetExternalAuthenticationSchemesAsync();
        ViewData["ExternalSchemes"] = externalSchemes.ToList();

        if (!ModelState.IsValid)
            return View("~/Views/Account/Login.cshtml", model);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.FirstOrDefault();

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            await auditService.LogLoginAttemptAsync(null, model.Email, ip, ua, false, "User not found");
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View("~/Views/Account/Login.cshtml", model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user, model.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            // MFA enabled — redirect to verification page, preserving return URL
            TempData["ReturnUrl"] = model.ReturnUrl;
            return RedirectToAction("VerifyMfa");
        }

        if (result.IsLockedOut)
        {
            await auditService.LogLoginAttemptAsync(user.Id, model.Email, ip, ua, false, "Account locked out");
            ModelState.AddModelError("", "Tài khoản đã bị khóa tạm thời. Vui lòng thử lại sau.");
            return View("~/Views/Account/Login.cshtml", model);
        }

        if (!result.Succeeded)
        {
            await auditService.LogLoginAttemptAsync(user.Id, model.Email, ip, ua, false, "Invalid password");
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View("~/Views/Account/Login.cshtml", model);
        }

        // Successful login
        await auditService.LogLoginAttemptAsync(user.Id, model.Email, ip, ua, true);

        // Redirect back to /connect/authorize with original OIDC params
        return LocalRedirect(model.ReturnUrl ?? "/");
    }

    // GET /account/verify-mfa — render MFA verification form
    [HttpGet("/account/verify-mfa")]
    public IActionResult VerifyMfa() =>
        View("~/Views/Account/VerifyMfa.cshtml", new VerifyMfaViewModel());

    // POST /account/verify-mfa — validate TOTP code
    [HttpPost("/account/verify-mfa")]
    public async Task<IActionResult> VerifyMfaPost(VerifyMfaViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Account/VerifyMfa.cshtml", model);

        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
            model.Code, isPersistent: false, rememberClient: false);

        if (result.Succeeded)
        {
            var returnUrl = TempData["ReturnUrl"]?.ToString() ?? "/";
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Tài khoản đã bị khóa tạm thời. Vui lòng thử lại sau.");
            return View("~/Views/Account/VerifyMfa.cshtml", model);
        }

        ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
        return View("~/Views/Account/VerifyMfa.cshtml", model);
    }

    // Build ClaimsIdentity for OpenIddict token generation (includes role claims)
    private async Task<ClaimsIdentity> CreateIdentity(ApplicationUser user)
    {
        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        identity.AddClaim(Claims.Subject, user.Id.ToString());
        identity.AddClaim(Claims.Email, user.Email ?? "");
        identity.AddClaim(Claims.Name, user.FullName);
        identity.AddClaim(Claims.PreferredUsername, user.UserName ?? user.Email ?? "");

        // Add role claims from ASP.NET Identity store
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
            identity.AddClaim(Claims.Role, role);

        // Add tenant claim if user belongs to a tenant
        if (user.TenantId.HasValue)
            identity.AddClaim("tenant_id", user.TenantId.Value.ToString());

        // Set destinations: which claims go into access_token vs id_token
        foreach (var claim in identity.Claims)
        {
            claim.SetDestinations(claim.Type switch
            {
                Claims.Name or Claims.PreferredUsername =>
                    new[] { Destinations.AccessToken, Destinations.IdentityToken },
                Claims.Email =>
                    new[] { Destinations.IdentityToken },
                // Roles must appear in both tokens — FE reads from id_token profile
                Claims.Role =>
                    new[] { Destinations.AccessToken, Destinations.IdentityToken },
                // Tenant ID needed by API for tenant-scoped operations
                "tenant_id" =>
                    new[] { Destinations.AccessToken, Destinations.IdentityToken },
                _ => new[] { Destinations.AccessToken }
            });
        }

        return identity;
    }
}
