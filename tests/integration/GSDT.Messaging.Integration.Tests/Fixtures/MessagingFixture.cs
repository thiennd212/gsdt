using GSDT.Infrastructure.Messaging;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace GSDT.Messaging.Integration.Tests.Fixtures;

/// <summary>
/// Shared fixture for messaging integration tests.
/// Wires MassTransit InMemory test harness + SQL Server Testcontainer for MessagingDbContext.
/// Transport: InMemory — matches env var MessageBus__Transport=InMemory used in CI.
/// All tests in [Collection("MessagingIntegration")] share one container cold-start.
/// </summary>
public sealed class MessagingFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Gov@Test1234!")
        .WithCleanUp(true)
        .Build();

    // Root DI container — resolved after harness starts
    public IServiceProvider Services { get; private set; } = null!;

    // MassTransit test harness — provides .Published, .Consumed, .Sent collections
    public ITestHarness Harness { get; private set; } = null!;

    public string SqlConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        // Build connection string with TrustServerCertificate for SqlClient 6.x
        var csb = new SqlConnectionStringBuilder(_sqlContainer.GetConnectionString())
        {
            TrustServerCertificate = true,
            Encrypt = SqlConnectionEncryptOption.Optional,
        };
        SqlConnectionString = csb.ConnectionString;

        // Ensure messaging schema exists (dead_letters table)
        await EnsureMessagingSchemaAsync();

        var services = new ServiceCollection();

        // MessagingDbContext — needed by DeadLetterConsumer and DeadLetterService
        services.AddDbContext<MessagingDbContext>(opts =>
            opts.UseSqlServer(SqlConnectionString));

        // MassTransit InMemory with test harness
        // AddMassTransitTestHarness replaces real bus with InMemoryTestHarness
        services.AddMassTransitTestHarness(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<DeadLetterConsumer>();
            x.AddConsumer<TestOrderConsumer>();
            x.AddConsumer<TestFaultingConsumer>();
        });

        services.AddScoped<DeadLetterService>();

        Services = services.BuildServiceProvider();
        Harness = Services.GetRequiredService<ITestHarness>();
        await Harness.Start();
    }

    public async Task DisposeAsync()
    {
        await Harness.Stop();
        await _sqlContainer.DisposeAsync();
    }

    private async Task EnsureMessagingSchemaAsync()
    {
        // MessagingDbContext has no EF migrations — use EnsureCreated to create the schema
        // and dead_letters table directly from the model (suitable for test-only DB).
        await using var db = new MessagingDbContext(
            new DbContextOptionsBuilder<MessagingDbContext>()
                .UseSqlServer(SqlConnectionString)
                .Options);
        await db.Database.EnsureCreatedAsync();
    }
}

[CollectionDefinition(CollectionName)]
public sealed class MessagingCollection : ICollectionFixture<MessagingFixture>
{
    public const string CollectionName = "MessagingIntegration";
}
