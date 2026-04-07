using FluentValidation;

namespace GSDT.Identity.Application;

/// <summary>Registers Identity Application layer: MediatR, validators, RBAC+ABAC policies.</summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MediatR handlers + validators
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        // RBAC + ABAC authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Admin, p => p.RequireRole(Roles.Admin, Roles.SystemAdmin))
            .AddPolicy(Policies.GovOfficer, p => p.RequireRole(Roles.GovOfficer, Roles.Admin, Roles.SystemAdmin))
            .AddPolicy(Policies.Citizen, p => p.RequireAuthenticatedUser())
            .AddPolicy(Policies.DepartmentRestricted, p =>
                p.AddRequirements(new DepartmentAccessRequirement()))
            .AddPolicy(Policies.ClassifiedAccess, p =>
                p.AddRequirements(new ClassificationAccessRequirement()));

        // ABAC authorization handlers
        services.AddScoped<IAuthorizationHandler, DepartmentAccessHandler>();
        services.AddScoped<IAuthorizationHandler, ClassificationAccessHandler>();

        return services;
    }
}
