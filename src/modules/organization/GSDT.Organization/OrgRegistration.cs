using FluentValidation;
using MediatR;

namespace GSDT.Organization;

/// <summary>DI registration for Organization module — called once from Program.cs.</summary>
public static class OrgRegistration
{
    public static IServiceCollection AddOrganizationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext — uses shared interceptors from InfrastructureRegistration
        services.AddDbContext<OrgDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "organization"));

            opts.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        // Core service — scoped (uses scoped DbContext)
        services.AddScoped<OrgUnitService>();

        // ITenantOrgContext — scoped: one per request, reads JWT claims
        services.AddScoped<ITenantOrgContext, JwtOrgContext>();

        // MediatR handlers — discovered from this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(OrgRegistration).Assembly));

        // FluentValidation — for future command validators in this assembly
        services.AddValidatorsFromAssembly(typeof(OrgRegistration).Assembly);

        return services;
    }
}
