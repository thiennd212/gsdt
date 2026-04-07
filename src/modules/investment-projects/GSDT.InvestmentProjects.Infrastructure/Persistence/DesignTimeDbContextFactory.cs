using Microsoft.EntityFrameworkCore.Design;

namespace GSDT.InvestmentProjects.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Used by `dotnet ef migrations add` when run from the Infrastructure project directory.
/// </summary>
internal sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<InvestmentProjectsDbContext>
{
    public InvestmentProjectsDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("Default")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GSDT_Dev;Trusted_Connection=True;";

        var optionsBuilder = new DbContextOptionsBuilder<InvestmentProjectsDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "investment"));

        // NoOp tenant context for design-time — filters inactive
        return new InvestmentProjectsDbContext(optionsBuilder.Options, new NoOpTenantContext());
    }
}

/// <summary>Design-time no-op ITenantContext — tenant ID irrelevant during migration generation.</summary>
internal sealed class NoOpTenantContext : ITenantContext
{
    public Guid? TenantId => null;
    public bool IsSystemAdmin => true; // Skip tenant filter during design-time migration
}
