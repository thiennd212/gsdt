
namespace GSDT.MasterData.Infrastructure.Persistence;

/// <summary>EF design-time factory — used by dotnet ef migrations add/update.</summary>
public class DesignTimeMasterDataDbContextFactory : IDesignTimeDbContextFactory<MasterDataDbContext>
{
    public MasterDataDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<MasterDataDbContext>()
            .UseSqlServer("Server=localhost;Database=GSDT;Trusted_Connection=True;")
            .Options;
        return new MasterDataDbContext(opts, new NoOpTenantContext());
    }

    // Minimal no-op for design-time — not used at runtime
    private sealed class NoOpTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
        public bool IsSystemAdmin => true;
    }
}
