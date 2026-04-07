using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace GSDT.Identity.Integration.Tests.Fixtures;

/// <summary>
/// Spins up a SQL Server 2022 container for the duration of the test collection.
/// Shared across all Identity integration tests — container starts once, tests run, container stops.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
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

    public Task InitializeAsync() => _container.StartAsync();
    public Task DisposeAsync() => _container.StopAsync();

    public SqlConnection OpenConnection() => new(ConnectionString);
}

/// <summary>xUnit collection definition — all Identity integration tests share one SQL Server container.</summary>
[CollectionDefinition(CollectionName)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string CollectionName = "SqlServer";
}
