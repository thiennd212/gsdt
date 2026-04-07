
namespace GSDT.Files.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tooling (migrations, script generation).
/// Reads connection string from environment or falls back to local dev defaults.
/// Only used by dotnet-ef CLI — not registered in the application DI container.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FilesDbContext>
{
    public FilesDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost,1433;Database=GSDT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true";

        var opts = new DbContextOptionsBuilder<FilesDbContext>()
            .UseSqlServer(connStr, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "files"))
            .Options;

        // Build minimal IConfiguration for design-time (encryption key not needed for migrations)
        var configuration = new ConfigurationBuilder().Build();
        return new FilesDbContext(opts, NullTenantContext.Instance, configuration);
    }
}
