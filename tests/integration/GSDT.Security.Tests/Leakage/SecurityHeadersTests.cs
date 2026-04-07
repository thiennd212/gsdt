using GSDT.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;

namespace GSDT.Security.Tests.Leakage;

/// <summary>
/// Security headers middleware tests — TC-SEC-LEAK-004.
/// Verifies OWASP security headers on all responses.
/// </summary>
public sealed class SecurityHeadersTests
{
    [Fact]
    [Trait("Category", "Security")]
    public async Task AllSecurityHeaders_SetOnApiResponse()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/cases";
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var h = context.Response.Headers;
        h["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        h["X-Frame-Options"].ToString().Should().Be("DENY");
        h["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
        h["Permissions-Policy"].ToString().Should().Be("camera=(), microphone=(), geolocation=()");
        h["X-XSS-Protection"].ToString().Should().Be("1; mode=block");
        h["Strict-Transport-Security"].ToString().Should().Contain("max-age=31536000");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task CspApiEndpoint_StrictPolicy()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/cases";
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'none'");
        csp.Should().Contain("frame-ancestors 'none'");
        csp.Should().NotContain("unsafe-inline");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task CspSwaggerEndpoint_RelaxedPolicy()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("unsafe-inline");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task CspScalarEndpoint_RelaxedPolicy()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/scalar/v1";
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var csp = context.Response.Headers["Content-Security-Policy"].ToString();
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("unsafe-inline");
    }
}
