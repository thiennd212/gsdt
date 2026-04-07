using System.Text.Json;
using GSDT.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GSDT.Security.Tests.Leakage;

/// <summary>
/// Error response leakage tests — TC-SEC-LEAK-001, TC-SEC-LEAK-003.
/// Ensures no stack traces or DB internals in production error responses.
/// </summary>
public sealed class ErrorResponseLeakageTests
{
    private static GlobalExceptionHandlerMiddleware CreateMiddleware(
        RequestDelegate throwingDelegate, bool isDevelopment = false)
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(isDevelopment ? "Development" : "Production");
        // IHostEnvironment.IsDevelopment() is an extension method checking EnvironmentName
        return new GlobalExceptionHandlerMiddleware(
            throwingDelegate,
            NullLogger<GlobalExceptionHandlerMiddleware>.Instance,
            env);
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task Production_NoStackTrace_InErrorResponse()
    {
        var middleware = CreateMiddleware(
            _ => throw new InvalidOperationException("SELECT * FROM Users WHERE Id = @id"),
            isDevelopment: false);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        body.Should().NotContain("SELECT");
        body.Should().NotContain("StackTrace");
        body.Should().NotContain("at GSDT");
        body.Should().Contain("Contact support");
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task Production_NoConnectionString_InErrorResponse()
    {
        var middleware = CreateMiddleware(
            _ => throw new Exception("Server=localhost;Database=AqtDb;User=sa;Password=secret123"),
            isDevelopment: false);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        body.Should().NotContain("Password");
        body.Should().NotContain("Server=");
        body.Should().NotContain("secret123");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task ErrorResponse_ContainsCorrelationId()
    {
        var middleware = CreateMiddleware(_ => throw new Exception("test"), isDevelopment: false);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var doc = JsonDocument.Parse(body);

        doc.RootElement.TryGetProperty("correlationId", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("errorCode", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task ArgumentException_Returns400_NotStackTrace()
    {
        var middleware = CreateMiddleware(
            _ => throw new ArgumentException("Invalid CaseId format"),
            isDevelopment: false);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().NotContain("CaseId format");
        body.Should().Contain("Invalid request parameter");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task UnauthorizedException_Returns401()
    {
        var middleware = CreateMiddleware(
            _ => throw new UnauthorizedAccessException("Token expired"),
            isDevelopment: false);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }
}
