
namespace GSDT.SystemParams.Infrastructure.Persistence;

/// <summary>EF design-time factory — used by dotnet ef migrations add/update.</summary>
public class DesignTimeSystemParamsDbContextFactory : IDesignTimeDbContextFactory<SystemParamsDbContext>
{
    public SystemParamsDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<SystemParamsDbContext>()
            .UseSqlServer("Server=localhost;Database=GSDT;Trusted_Connection=True;")
            .Options;
        return new SystemParamsDbContext(opts, new NoOpTenantContext());
    }

    private sealed class NoOpTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
        public bool IsSystemAdmin => true;
    }
}
