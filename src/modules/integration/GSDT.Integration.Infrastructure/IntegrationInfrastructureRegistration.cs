
namespace GSDT.Integration.Infrastructure;

public static class IntegrationInfrastructureRegistration
{
    public static IServiceCollection AddIntegrationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IntegrationDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "integration"));

            options.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IMessageLogRepository, MessageLogRepository>();

        return services;
    }
}
