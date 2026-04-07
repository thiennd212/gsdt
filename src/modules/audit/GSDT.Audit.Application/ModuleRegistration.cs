using FluentValidation;

namespace GSDT.Audit.Application;

/// <summary>Registers Audit Application layer services — MediatR handlers + validators.</summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddAuditModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        return services;
    }
}
