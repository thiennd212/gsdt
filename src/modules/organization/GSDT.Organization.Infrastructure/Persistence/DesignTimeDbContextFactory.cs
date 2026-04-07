
namespace GSDT.Organization.Infrastructure.Persistence;

/// <summary>EF design-time factory — used by dotnet ef migrations add/update.</summary>
public class DesignTimeOrgDbContextFactory : IDesignTimeDbContextFactory<OrgDbContext>
{
    public OrgDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<OrgDbContext>()
            .UseSqlServer("Server=localhost;Database=GSDT;Trusted_Connection=True;")
            .Options;
        return new OrgDbContext(opts, new NoOpTenantContext());
    }

    private sealed class NoOpTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
        public bool IsSystemAdmin => true;
    }
}
