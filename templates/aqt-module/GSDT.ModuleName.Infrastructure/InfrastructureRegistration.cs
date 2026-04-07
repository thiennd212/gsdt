using GSDT.Infrastructure.Persistence;
using GSDT.ModuleName.Domain.Repositories;
using GSDT.ModuleName.Infrastructure.Persistence;
using GSDT.ModuleName.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GSDT.ModuleName.Infrastructure;

/// <summary>DI registration for ModuleName module Infrastructure layer.</summary>
public static class ModuleNameInfrastructureRegistration
{
    public static IServiceCollection AddModuleNameInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ModuleNameDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "modulename"));

            opts.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IModuleNameRepository, ModuleNameRepository>();

        return services;
    }
}
