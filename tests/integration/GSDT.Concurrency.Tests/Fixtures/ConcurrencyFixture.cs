using GSDT.Cases.Infrastructure.Persistence;
using GSDT.SharedKernel.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace GSDT.Concurrency.Tests.Fixtures;

/// <summary>
/// Starts a SQL Server Testcontainers instance and runs EF migrations for the Cases module.
/// Shared across all tests in the Concurrency collection — container starts once per test run.
/// </summary>
public sealed class ConcurrencyFixture : IAsyncLifetime
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

        // Run migrations once so all tests in this collection share a migrated schema.
        await using var ctx = CreateContext(new TestTenantContext(null, isSystemAdmin: true));
        await ctx.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.StopAsync();

    /// <summary>
    /// Returns a fresh, independent <see cref="CasesDbContext"/> for each call.
    /// Callers are responsible for disposal.
    /// Service-provider caching is disabled so EF compiles a new model per context
    /// (required when the global filter captures a different ITenantContext each time).
    /// </summary>
    public CasesDbContext CreateContext(ITenantContext tenantContext)
    {
        var opts = new DbContextOptionsBuilder<CasesDbContext>()
            .UseSqlServer(ConnectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "cases"))
            .EnableServiceProviderCaching(false)
            .Options;

        return new CasesDbContext(opts, tenantContext);
    }
}

[CollectionDefinition(CollectionName)]
public sealed class ConcurrencyCollection : ICollectionFixture<ConcurrencyFixture>
{
    public const string CollectionName = "Concurrency";
}
