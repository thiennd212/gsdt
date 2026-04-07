using GSDT.ModuleName.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GSDT.ModuleName.Infrastructure.Persistence;

public sealed class ModuleNameDbContext(DbContextOptions<ModuleNameDbContext> options)
    : DbContext(options)
{
    public DbSet<ModuleNameEntity> ModuleNames => Set<ModuleNameEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("modulename");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModuleNameDbContext).Assembly);
    }
}
