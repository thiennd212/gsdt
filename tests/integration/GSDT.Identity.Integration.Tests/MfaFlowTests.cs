using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Integration.Tests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GSDT.Identity.Integration.Tests;

/// <summary>
/// Integration tests for MFA (TOTP) flow.
/// Controller: GET /api/v1/mfa/setup, POST /api/v1/mfa/verify, POST /api/v1/mfa/send-otp
/// No userId in route — user identity derived from auth token (X-Test-UserId header).
/// Tests run against Testcontainers SQL Server via WebAppFixture.
/// </summary>
[Collection(SqlServerCollection.CollectionName)]
public sealed class MfaFlowTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _app;
    private static readonly Guid TestTenantId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    public MfaFlowTests(WebAppFixture app)
    {
        _app = app;
    }

    /// <summary>Seeds an ApplicationUser into Identity DB so MFA service can find the user.</summary>
    private async Task SeedTestUserAsync()
    {
        using var scope = _app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existing = await userManager.FindByIdAsync(TestUserId.ToString());
        if (existing != null) return;

        var user = new ApplicationUser
        {
            Id = TestUserId,
            UserName = $"testmfa-{TestUserId:N}",
            Email = "testmfa@test.vn",
            EmailConfirmed = true,
            FullName = "MFA Test User",
            TenantId = TestTenantId,
            IsActive = true,
        };
        var result = await userManager.CreateAsync(user, "TestPass@12345!");
        result.Succeeded.Should().BeTrue($"Failed to seed user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    // --- Setup TOTP ---

    [Fact]
    public async Task SetupMfa_AuthenticatedUser_Returns200WithProvisioningUri()
    {
        await SeedTestUserAsync();
        var client = _app.CreateAuthenticatedClient(TestUserId, TestTenantId);

        // Controller: GET /api/v1/mfa/setup
        var response = await client.GetAsync("/api/v1/mfa/setup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Controller returns: { otpauthUri: "otpauth://totp/..." }
        var uri = body.GetProperty("otpauthUri").GetString();
        uri.Should().StartWith("otpauth://totp/");
        uri.Should().Contain("secret=");
    }

    [Fact]
    public async Task SetupMfa_AuthenticatedUser_ReturnsValidOtpauthUri()
    {
        await SeedTestUserAsync();
        var client = _app.CreateAuthenticatedClient(TestUserId, TestTenantId);

        var response = await client.GetAsync("/api/v1/mfa/setup");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        var uri = body.GetProperty("otpauthUri").GetString()!;

        // Must be parseable as a URI and contain required TOTP parameters
        var parsed = new Uri(uri);
        parsed.Scheme.Should().Be("otpauth");
        uri.Should().Contain("digits=6");
        uri.Should().Contain("period=30");
    }

    [Fact]
    public async Task SetupMfa_UnauthenticatedRequest_Returns401()
    {
        var client = _app.CreateClient();

        var response = await client.GetAsync("/api/v1/mfa/setup");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- Verify TOTP code ---

    [Fact]
    public async Task VerifyMfa_ValidTotpCode_Returns200()
    {
        await SeedTestUserAsync();
        var client = _app.CreateAuthenticatedClient(TestUserId, TestTenantId);

        // Step 1: set up MFA — GET returns otpauthUri containing the base32 secret
        var setupResp = await client.GetAsync("/api/v1/mfa/setup");
        setupResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var setupBody = await setupResp.Content.ReadFromJsonAsync<JsonElement>();
        var otpauthUri = setupBody.GetProperty("otpauthUri").GetString()!;

        // Step 2: extract shared secret from otpauth URI query string (secret= param)
        // Use Uri.Query split — avoids System.Web dependency, works on all platforms
        var queryPairs = new Uri(otpauthUri).Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0], p => p.Length > 1 ? Uri.UnescapeDataString(p[1]) : "");
        var sharedSecret = queryPairs["secret"];
        sharedSecret.Should().NotBeNullOrEmpty();

        // Step 3: generate current TOTP code using Otp.NET (same algorithm as server)
        var secretBytes = Base32Encoding.ToBytes(sharedSecret);
        var totp = new Totp(secretBytes);
        var currentCode = totp.ComputeTotp();

        // Step 4: verify — POST /api/v1/mfa/verify { "Code": "123456" }
        var verifyResponse = await client.PostAsJsonAsync(
            "/api/v1/mfa/verify",
            new { Code = currentCode });

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<JsonElement>();
        verifyBody.GetProperty("verified").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task VerifyMfa_InvalidCode_Returns400()
    {
        await SeedTestUserAsync();
        var client = _app.CreateAuthenticatedClient(TestUserId, TestTenantId);

        // Ensure setup is done first so a key exists
        await client.GetAsync("/api/v1/mfa/setup");

        // Controller returns 400 BadRequest for invalid TOTP code
        var verifyResponse = await client.PostAsJsonAsync(
            "/api/v1/mfa/verify",
            new { Code = "000000" }); // always invalid

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyMfa_EmptyCode_Returns400()
    {
        await SeedTestUserAsync();
        var client = _app.CreateAuthenticatedClient(TestUserId, TestTenantId);

        // Empty code — controller validates and returns 400
        var verifyResponse = await client.PostAsJsonAsync(
            "/api/v1/mfa/verify",
            new { Code = "" });

        // Controller calls ValidateTotpAsync which returns false for empty code → 400
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Send Email OTP ---

    [Fact]
    public async Task SendOtp_AuthenticatedUser_Returns202()
    {
        await SeedTestUserAsync();
        var client = _app.CreateAuthenticatedClient(TestUserId, TestTenantId);

        // POST /api/v1/mfa/send-otp — enqueues Hangfire job, returns 202 Accepted
        var response = await client.PostAsync("/api/v1/mfa/send-otp", null);

        // 202 Accepted or 200 OK depending on whether user exists in DB
        // In integration test, test user may not exist in Identity DB → service logs warning and returns Accepted
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Accepted, HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendOtp_UnauthenticatedRequest_Returns401()
    {
        var client = _app.CreateClient();

        var response = await client.PostAsync("/api/v1/mfa/send-otp", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
