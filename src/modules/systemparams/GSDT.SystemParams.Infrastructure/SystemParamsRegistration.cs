using FluentValidation;

namespace GSDT.SystemParams.Infrastructure;

/// <summary>DI registration for SystemParams module — called once from Program.cs.</summary>
public static class SystemParamsRegistration
{
    public static IServiceCollection AddSystemParams(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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

        services.AddSingleton<ISystemParamService, SystemParamService>();

        services.AddSingleton<FeatureFlagService>();
        services.AddSingleton<IFeatureFlagService>(sp => sp.GetRequiredService<FeatureFlagService>());
        services.AddSingleton<SharedKernel.Contracts.IFeatureFlagService>(sp => sp.GetRequiredService<FeatureFlagService>());

        services.AddHostedService<FeatureFlagRefreshService>();
        services.AddHostedService(sp =>
            new FeatureFlagSubscriber(
                sp.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>(),
                sp.GetRequiredService<IFeatureFlagService>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<FeatureFlagSubscriber>>()));
        services.AddHostedService<SystemParamSeeder>();

        return services;
    }
}
