using FluentValidation;
using MediatR;
using System.Security.Claims;

namespace GSDT.Organization.Infrastructure;

/// <summary>DI registration for Organization module — called once from Program.cs.</summary>
public static class OrgRegistration
{
    public static IServiceCollection AddOrganizationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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

        services.AddScoped<OrgUnitService>();
        services.AddScoped<ITenantOrgContext, JwtOrgContext>();
        services.AddScoped<IOrgUnitModuleClient, InProcessOrgUnitModuleClient>();

        // MediatR: commands/queries in Application; EF handlers in Infrastructure
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Commands.CreateOrgUnitCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(OrgRegistration).Assembly);
        });
        services.AddValidatorsFromAssembly(typeof(Application.Commands.CreateOrgUnitCommand).Assembly);

        return services;
    }
}

/// <summary>
/// Reads org_unit_id and tenant_id JWT claims; resolves ancestor hierarchy via
/// OrgUnitService (cache-backed — safe for sync-over-async after first request).
/// </summary>
internal sealed class JwtOrgContext(IHttpContextAccessor accessor, OrgUnitService orgService)
    : ITenantOrgContext
{
    private IReadOnlyList<Guid>? _cachedHierarchy;

    public Guid? CurrentOrgUnitId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue("org_unit_id"), out var id)
            ? id : null;

    public bool IsInOrgUnit(Guid orgUnitId) =>
        GetOrgUnitHierarchy().Contains(orgUnitId);

    public IReadOnlyList<Guid> GetOrgUnitHierarchy()
    {
        if (_cachedHierarchy is not null) return _cachedHierarchy;

        if (CurrentOrgUnitId is null)
        {
            _cachedHierarchy = [];
            return _cachedHierarchy;
        }

        var tenantIdStr = accessor.HttpContext?.User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdStr, out var tenantId))
        {
            _cachedHierarchy = [];
            return _cachedHierarchy;
        }

        _cachedHierarchy = orgService
            .GetAncestorsAsync(CurrentOrgUnitId.Value, tenantId)
            .GetAwaiter()
            .GetResult();

        return _cachedHierarchy;
    }
}
