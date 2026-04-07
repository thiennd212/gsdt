
namespace GSDT.Organization.Persistence;

/// <summary>
/// Design-time factory for EF Core tooling (migrations, script generation).
/// Reads connection string from environment or falls back to local dev defaults.
/// Only used by dotnet-ef CLI — not registered in the application DI container.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrgDbContext>
{
    public OrgDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost,1433;Database=GSDT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true";

        var opts = new DbContextOptionsBuilder<OrgDbContext>()
            .UseSqlServer(connStr, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "organization"))
            .Options;

        return new OrgDbContext(opts, NullTenantContext.Instance);
    }
}
