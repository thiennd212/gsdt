using GSDT.InvestmentProjects.Application;
using GSDT.InvestmentProjects.Application.Services;
using GSDT.InvestmentProjects.Domain.Repositories;
using GSDT.InvestmentProjects.Infrastructure.Persistence;
using GSDT.InvestmentProjects.Infrastructure.Persistence.Repositories;
using GSDT.InvestmentProjects.Infrastructure.Services;

namespace GSDT.InvestmentProjects.Infrastructure;

/// <summary>DI registration for the InvestmentProjects module — infrastructure + application layers.</summary>
public static class InvestmentProjectsRegistration
{
    public static IServiceCollection AddInvestmentProjects(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddInvestmentProjectsApplication();
        services.AddDbContext<InvestmentProjectsDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(
                config.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "investment"));

            opts.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        services.AddScoped<IInvestmentProjectRepository, InvestmentProjectRepository>();
        services.AddScoped<IProjectQueryScopeService, ProjectQueryScopeService>();

        return services;
    }
}
