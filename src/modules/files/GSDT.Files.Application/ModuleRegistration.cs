using FluentValidation;

namespace GSDT.Files.Application;

public static class ModuleRegistration
{
    public static IServiceCollection AddFilesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FilesOptions>(configuration.GetSection(FilesOptions.SectionName));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        return services;
    }
}
