using GSDT.Files.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using System.Security.Claims;

namespace GSDT.Files.Integration.Tests.Fixtures;

/// <summary>
/// WebApplicationFactory wired to Testcontainers SQL Server.
/// Overrides:
///   - DB connection strings → Testcontainers instance
///   - Auth → TestAuthHandler (bypasses JWT validation in tests)
///   - IFileStorageService → InMemory stub (no MinIO needed in tests)
///   - IVirusScanner → stub that marks files clean immediately
///   - Background services → disabled to avoid Hangfire noise
/// </summary>
public sealed class WebAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqlServerFixture _sqlServer;

    public WebAppFixture(SqlServerFixture sqlServer)
    {
        _sqlServer = sqlServer;
    }

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
                // Bypass ClamAV — no antivirus container in tests
                ["ClamAv:BypassWhenUnavailable"] = "true",
                // Redis: keep default (redis-1 container running locally)
                ["Redis:ConnectionString"] = string.Empty,
                // Disable AI health probes
                ["AI:Ollama:Enabled"] = "false",
                ["Gateway:Mode"] = "Disabled",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace auth with test bypass scheme
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

            // Disable all background services (Hangfire, seeders, ClamAV health check)
            services.RemoveAll<IHostedService>();

            // Replace IFileStorageService with in-memory stub — no MinIO required
            services.RemoveAll<IFileStorageService>();
            services.AddSingleton<IFileStorageService>(new InMemoryFileStorageService());

            // Replace IVirusScanner with a stub that always returns clean
            services.RemoveAll<IVirusScanner>();
            var stubScanner = Substitute.For<IVirusScanner>();
            stubScanner.ScanAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(VirusScanResult.Clean()));
            services.AddSingleton(stubScanner);
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
        // Environment variables override appsettings.json — required for EF/Hangfire lambdas
        // that capture connection strings at AddDbContext registration time.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _sqlServer.ConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Hangfire", _sqlServer.ConnectionString);
        Environment.SetEnvironmentVariable("Vault__Enabled", "false");
        Environment.SetEnvironmentVariable("MessageBus__Transport", "InMemory");
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
/// Test-only auth handler — reads claims from request headers, no JWT validation needed.
/// Headers: X-Test-UserId, X-Test-Roles (comma-separated), X-Test-TenantId.
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

/// <summary>
/// In-memory IFileStorageService — stores files in a ConcurrentDictionary by storage key.
/// Allows upload/download/delete tests without a running MinIO instance.
/// </summary>
public sealed class InMemoryFileStorageService : IFileStorageService
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte[]> _store = new();

    public Task<string> UploadAsync(
        Stream stream,
        string bucket,
        string key,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        _store[key] = ms.ToArray();
        return Task.FromResult(key);
    }

    public Task<Stream> DownloadAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(key, out var data))
            throw new KeyNotFoundException($"Key '{key}' not found in in-memory store.");
        return Task.FromResult<Stream>(new MemoryStream(data));
    }

    public Task DeleteAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
