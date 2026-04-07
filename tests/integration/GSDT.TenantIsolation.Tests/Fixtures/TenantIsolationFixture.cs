using GSDT.Cases.Infrastructure.Persistence;
using GSDT.SharedKernel.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace GSDT.TenantIsolation.Tests.Fixtures;

/// <summary>
/// Starts a SQL Server Testcontainers instance and runs EF migrations for the Cases module.
/// Shared across all tests in the TenantIsolation collection — container starts once.
/// </summary>
public sealed class TenantIsolationFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Gov@Test1234!")
        .Build();

    /// <summary>
    /// Connection string with TrustServerCertificate=True — required for SqlClient 6.x with Testcontainers.
    /// </summary>
    public string ConnectionString
    {
        get
        {
            var csb = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
                TrustServerCertificate = true,
                Encrypt = SqlConnectionEncryptOption.Optional,
            };
            return csb.ConnectionString;
        }
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Run EF migrations once so all tests share a migrated schema.
        // Use NullTenantContext (IsSystemAdmin=true) so the migration setup sees all data.
        await using var ctx = CreateContext(new TestTenantContext(null, isSystemAdmin: true));
        await ctx.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.StopAsync();

    /// <summary>
    /// Creates a <see cref="CasesDbContext"/> scoped to the provided <paramref name="tenantContext"/>.
    /// Each call returns a fresh, independent context — callers are responsible for disposal.
    /// </summary>
    public CasesDbContext CreateContext(ITenantContext tenantContext)
    {
        var opts = new DbContextOptionsBuilder<CasesDbContext>()
            .UseSqlServer(ConnectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "cases"))
            // Disable model cache so each context gets a fresh compiled model
            // with its own captured ITenantContext constant.
            // Without this, EF re-uses the first-built model across all contexts
            // in the process — the global filter would always see the migration-time
            // NullTenantContext (IsSystemAdmin=true) regardless of what we pass here.
            .EnableServiceProviderCaching(false)
            .Options;

        return new CasesDbContext(opts, tenantContext);
    }
}

[CollectionDefinition(CollectionName)]
public sealed class TenantIsolationCollection : ICollectionFixture<TenantIsolationFixture>
{
    public const string CollectionName = "TenantIsolation";
}
