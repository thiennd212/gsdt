using Xunit;

namespace GSDT.Tests.Integration.Infrastructure;

/// <summary>
/// Base class for all integration tests.
/// Injects DatabaseFixture via xUnit collection fixture — shared across all test classes.
/// Factory is accessed via db.Factory (set after migrations + seeders complete in InitializeAsync).
/// Default Client is pre-authenticated as SystemAdmin.
/// </summary>
[Collection("Integration")]
public abstract class IntegrationTestBase(DatabaseFixture db) : IAsyncLifetime
{
    protected ApiFactory Factory { get; } = db.Factory;

    /// <summary>Pre-authenticated HttpClient (SystemAdmin role). Set in InitializeAsync.</summary>
    protected HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// Creates an authenticated HttpClient with the specified identity.
    /// Caller is responsible for disposing.
    /// </summary>
    protected HttpClient CreateAuthenticatedClient(
        string? userId = null,
        string[]? roles = null,
        string? tenantId = null,
        string? managingAuthorityId = null,
        string? projectOwnerId = null) =>
        Factory.CreateAuthenticatedClient(userId, roles, tenantId, managingAuthorityId, projectOwnerId);

    // Fixed tenant id used for the default SystemAdmin client.
    // Must be provided because controllers resolve tenant from JWT claims (F-01 rule)
    // and return 403 when tenantContext.TenantId is null.
    protected static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public virtual Task InitializeAsync()
    {
        // Admin + SystemAdmin roles: Admin provides all 23 permissions via DB lookup,
        // SystemAdmin satisfies [Authorize(Roles = "SystemAdmin")] on specific endpoints.
        // Auto-resolves to seeded admin user via ApiFactory.CreateAuthenticatedClient.
        Client = CreateAuthenticatedClient(roles: ["Admin", "SystemAdmin"], tenantId: DefaultTenantId.ToString());
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }
}
