using FluentValidation;
using MediatR;

namespace GSDT.MasterData;

/// <summary>DI registration for MasterData module — called once from Program.cs.</summary>
public static class MasterDataRegistration
{
    public static IServiceCollection AddMasterData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MediatR handlers + validators from this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(MasterDataRegistration).Assembly));
        services.AddValidatorsFromAssembly(typeof(MasterDataRegistration).Assembly);

        // DbContext — uses shared interceptors from InfrastructureRegistration
        services.AddDbContext<MasterDataDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "masterdata"));

            opts.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        // AdministrativeUnitService — singleton (in-memory map, BuildCacheAsync called by seeder)
        services.AddSingleton<AdministrativeUnitService>();
        services.AddSingleton<IAdministrativeUnitService>(
            sp => sp.GetRequiredService<AdministrativeUnitService>());

        // Startup seeder — synchronous (app waits: 3-5s first boot, fast on restarts)
        services.AddHostedService<MasterDataSeeder>();

        // OutputCache policies (in-memory — supports tag eviction; NOT Redis OutputCache)
        services.AddOutputCache(opts =>
        {
            opts.AddPolicy("MasterData", b => b
                .Expire(TimeSpan.FromMinutes(30))
                .Tag("masterdata"));

            opts.AddPolicy("AdminUnits", b => b
                .Expire(TimeSpan.FromMinutes(30))
                .Tag("admin-units")
                .SetVaryByQuery("level", "parentCode"));
        });

        return services;
    }
}
