using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace GSDT.Audit.Integration.Tests.Fixtures;

/// <summary>
/// WebApplicationFactory wired to Testcontainers SQL Server.
/// Overrides DB connections, auth (TestAuthHandler), and disables background services.
/// </summary>
public sealed class WebAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqlServerFixture _sqlServer;

    public WebAppFixture(SqlServerFixture sqlServer) => _sqlServer = sqlServer;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _sqlServer.ConnectionString,
                ["ConnectionStrings:Hangfire"] = _sqlServer.ConnectionString,
                ["Vault:Enabled"] = "false",
                ["MessageBus:Transport"] = "InMemory",
                ["Redis:ConnectionString"] = string.Empty,
                ["AI:Ollama:Enabled"] = "false",
                ["Gateway:Mode"] = "Disabled", // prevents YARP + rate-limiter policy conflict in tests
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthHandler.SchemeName;
            });

            services.RemoveAll<IHostedService>();
        });
    }

    /// <summary>Creates an HttpClient pre-authenticated for the given tenant and roles.</summary>
    public HttpClient CreateAuthenticatedClient(Guid userId, Guid tenantId, params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, tenantId.ToString());
        if (roles.Length > 0)
            client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(",", roles));
        return client;
    }

    public Task InitializeAsync()
    {
        // Environment variables have highest priority in .NET config (override appsettings.json).
        // ConfigureAppConfiguration on IWebHostBuilder only reaches the web host config layer,
        // not builder.Configuration (ConfigurationManager) used by AddDbContext lambdas.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _sqlServer.ConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Hangfire", _sqlServer.ConnectionString);
        Environment.SetEnvironmentVariable("Vault__Enabled", "false");
        Environment.SetEnvironmentVariable("MessageBus__Transport", "InMemory");
        // Redis: keep default localhost:6379 from appsettings.json (redis-1 container is running)
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__Hangfire", null);
        Environment.SetEnvironmentVariable("Vault__Enabled", null);
        Environment.SetEnvironmentVariable("MessageBus__Transport", null);
        base.Dispose();
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test-only auth handler — reads claims from separate headers, no JWT needed.
/// Uses X-Test-UserId, X-Test-Roles, X-Test-TenantId headers.
/// </summary>
public sealed class TestAuthHandler(
    Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
    Microsoft.Extensions.Logging.ILoggerFactory logger,
    System.Text.Encodings.Web.UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestAuth";
    public const string UserIdHeader = "X-Test-UserId";
    public const string RolesHeader = "X-Test-Roles";
    public const string TenantIdHeader = "X-Test-TenantId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(UserIdHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var userId = Request.Headers[UserIdHeader].FirstOrDefault()!;
        var roles = Request.Headers[RolesHeader].FirstOrDefault()?.Split(',') ?? [];
        var tenantId = Request.Headers[TenantIdHeader].FirstOrDefault();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@test.vn"),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        if (tenantId != null) claims.Add(new("tenant_id", tenantId));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, SchemeName)));
    }
}
