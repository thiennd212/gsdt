using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GSDT.ModuleName.Application;

/// <summary>DI registration for ModuleName module Application layer.</summary>
public static class ModuleNameRegistration
{
    public static IServiceCollection AddModuleNameModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register module-specific application services here
        return services;
    }
}
