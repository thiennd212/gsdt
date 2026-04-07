
namespace GSDT.Integration.Infrastructure.Persistence;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IntegrationDbContext>
{
    public IntegrationDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost,1433;Database=GSDT;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true";

        var opts = new DbContextOptionsBuilder<IntegrationDbContext>()
            .UseSqlServer(connStr, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "integration"))
            .Options;

        return new IntegrationDbContext(opts, NullTenantContext.Instance);
    }
}
