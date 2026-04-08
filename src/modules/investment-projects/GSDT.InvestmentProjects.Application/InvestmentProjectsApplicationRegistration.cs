namespace GSDT.InvestmentProjects.Application;

/// <summary>DI registration for the InvestmentProjects Application layer.</summary>
public static class InvestmentProjectsApplicationRegistration
{
    public static IServiceCollection AddInvestmentProjectsApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(InvestmentProjectsApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof(InvestmentProjectsApplicationRegistration).Assembly,
            includeInternalTypes: true);

        return services;
    }
}
