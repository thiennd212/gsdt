using FluentValidation;

namespace GSDT.SystemParams;

/// <summary>DI registration for SystemParams module — called once from Program.cs.</summary>
public static class SystemParamsRegistration
{
    public static IServiceCollection AddSystemParams(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext — uses shared interceptors from InfrastructureRegistration
        services.AddDbContext<SystemParamsDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "config"));

            opts.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        // SystemParamService — singleton (IServiceScopeFactory for scoped EF access internally)
        services.AddSingleton<ISystemParamService, SystemParamService>();

        // FeatureFlagService — singleton (ConcurrentDict L0 cache)
        services.AddSingleton<FeatureFlagService>();
        services.AddSingleton<IFeatureFlagService>(sp => sp.GetRequiredService<FeatureFlagService>());
        // Cross-module contract — consumed by other modules via SharedKernel.Contracts
        services.AddSingleton<SharedKernel.Contracts.IFeatureFlagService>(sp => sp.GetRequiredService<FeatureFlagService>());

        // Background services for feature flag propagation
        services.AddHostedService<FeatureFlagRefreshService>();
        services.AddHostedService<FeatureFlagSubscriber>();

        // Startup seeder (idempotent — seeds 5 params + 3 feature flags)
        services.AddHostedService<SystemParamSeeder>();

        // Legacy validators (SystemParams assembly — old controllers)
        services.AddScoped<IValidator<CreateAnnouncementRequest>, CreateAnnouncementRequestValidator>();
        // NOTE: Presentation assembly validators registered in Program.cs via AddValidatorsFromAssemblyContaining

        return services;
    }
}
