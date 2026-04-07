
namespace GSDT.MasterData.Persistence;

/// <summary>
/// Design-time factory for EF Core tooling (migrations, script generation).
/// Reads connection string from environment or falls back to local dev defaults.
/// Only used by dotnet-ef CLI — not registered in the application DI container.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MasterDataDbContext>
{
    public MasterDataDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost,1433;Database=GSDT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true";

        var opts = new DbContextOptionsBuilder<MasterDataDbContext>()
            .UseSqlServer(connStr, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "masterdata"))
            .Options;

        return new MasterDataDbContext(opts, NullTenantContext.Instance);
    }
}
