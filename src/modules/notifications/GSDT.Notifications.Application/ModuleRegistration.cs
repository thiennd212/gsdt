using FluentValidation;

namespace GSDT.Notifications.Application;

public static class ModuleRegistration
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        // Cross-module client — in-process adapter for monolith mode
        services.AddScoped<INotificationModuleClient, InProcessNotificationModuleClient>();

        return services;
    }
}
