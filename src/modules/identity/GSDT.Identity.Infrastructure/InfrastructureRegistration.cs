using Hangfire;
using MediatR;

namespace GSDT.Identity.Infrastructure;

/// <summary>
/// Registers all Identity Infrastructure services.
/// Call from Api host: services.AddIdentityInfrastructure(configuration, env)
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        // --- MediatR handlers in Infrastructure assembly (DbContext-dependent handlers) ---
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(InfrastructureRegistration).Assembly));

        // --- EF Core ---
        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity"));

            // OpenIddict EF Core stores registered on IdentityDbContext
            options.UseOpenIddict();

            options.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        // --- ASP.NET Core Identity ---
        services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
            {
                // Password policy per QĐ742 — teams tighten via configuration
                opts.Password.RequiredLength = 12;
                opts.Password.RequireNonAlphanumeric = true;
                opts.Password.RequireDigit = true;
                opts.Password.RequireUppercase = true;
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opts.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        // --- OpenIddict (resource server validation — remote discovery via AuthServer) ---
        services.AddOpenIddict()
            .AddCore(opts => opts.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>())
            .AddValidation(opts =>
            {
                // AuthorityUrl must match the issuer AuthServer advertises in discovery doc.
                // In Docker: use host.docker.internal to reach the host-mapped AuthServer port.
                // Local dev: use http://localhost:5002 (AuthServer local port).
                var authority = configuration["Auth:AuthorityUrl"]
                    ?? throw new InvalidOperationException("Auth:AuthorityUrl is required.");
                opts.SetIssuer(authority);

                // Use introspection for reference token validation — API sends token
                // to AuthServer's /connect/introspect endpoint for verification
                var clientId = configuration["Auth:ResourceClientId"] ?? "gsdt-api-resource";
                var clientSecret = configuration["Auth:ResourceClientSecret"] ?? "dev-resource-secret-change-in-prod";
                opts.UseIntrospection()
                    .SetClientId(clientId)
                    .SetClientSecret(clientSecret);

                opts.UseSystemNetHttp();
                opts.UseAspNetCore();
            });

        // --- Repositories ---
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccessReviewRepository, AccessReviewRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IConsentRepository, ConsentRepository>();
        services.AddScoped<IDelegationRepository, DelegationRepository>();
        services.AddScoped<IAttributeRuleRepository, AttributeRuleRepository>();
        services.AddScoped<IExternalIdentityRepository, ExternalIdentityRepository>();
        services.AddScoped<ICredentialPolicyRepository, CredentialPolicyRepository>();
        services.AddScoped<IJitProviderConfigRepository, JitProviderConfigRepository>();

        // --- Claims enrichment (roles + delegation — cached in Redis) ---
        services.AddTransient<IClaimsTransformation, ClaimsEnrichmentTransformer>();

        // --- MFA (TOTP + Email OTP via Hangfire) ---
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<EmailOtpJob>();

        // --- VNeID connector (stub for Development/Testing; production requires real VNeIdHttpConnector) ---
        if (env.IsDevelopment() || env.IsEnvironment("Testing"))
            services.AddSingleton<IVneIdConnector, VNeIdStubConnector>();
        else
            throw new InvalidOperationException(
                "VNeIdHttpConnector is not implemented. Register a production IVneIdConnector before deploying.");

        // --- Password expiry (QĐ742 policy) ---
        services.AddScoped<PasswordExpiryService>();

        // --- Bulk role cache invalidation (Hangfire job) ---
        services.AddScoped<BulkRoleChangeService>();

        // --- Permission version counter (Redis atomic increment for cache invalidation) ---
        services.AddScoped<IPermissionVersionService, PermissionVersionService>();

        // --- Delegation expiry job (Hangfire recurring — marks overdue Active delegations as Expired) ---
        services.AddScoped<DelegationExpiryJob>();

        // --- Access review scheduler (Hangfire recurring — every 6 months) ---
        services.AddScoped<AccessReviewSchedulerJob>();

        // --- ABAC authorization handler (IMemoryCache for attribute rules) ---
        services.AddMemoryCache();
        services.AddScoped<IAuthorizationHandler, AbacAuthorizationHandler>();
        services.AddScoped<IAttributeRuleReader, AttributeRuleReaderAdapter>();
        // Cache invalidator — inject into any AttributeRule write command to bust stale ABAC cache (F-17)
        services.AddSingleton<IAbacCacheInvalidator, AbacCacheInvalidator>();

        // --- Data scope resolution (role + user-override merge, Redis-cached) ---
        services.AddScoped<IDataScopeService, DataScopeService>();

        // --- Policy rule evaluation (Redis-cached, TTL 5 min) ---
        services.AddScoped<IPolicyRuleEvaluator, PolicyRuleEvaluator>();

        // --- Effective permission resolution (direct + group + delegation, Redis-cached TTL 10 min) ---
        services.AddScoped<IEffectivePermissionService, EffectivePermissionService>();

        // --- SoD conflict detection (compares user's effective perms vs candidate role perms) ---
        services.AddScoped<ISodConflictChecker, SodConflictChecker>();

        // --- Menu authorization (permission-filtered tree, Redis-cached TTL 30 min per tenant) ---
        services.AddScoped<IMenuService, MenuService>();

        // --- Permission-code authorization handler ---
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // RTBF PII anonymizer
        services.AddScoped<IModulePiiAnonymizer, IdentityPiiAnonymizer>();

        // Cross-module identity lookup (used by Audit RTBF, etc.)
        services.AddScoped<SharedKernel.Contracts.Clients.IIdentityModuleClient, InProcessIdentityModuleClient>();

        // Cross-module PDPL consent recording (used by Forms module)
        services.AddScoped<SharedKernel.Contracts.Clients.IConsentModuleClient, InProcessConsentModuleClient>();

        return services;
    }

    /// <summary>
    /// Registers Hangfire recurring jobs for the Identity module.
    /// Call from the host after UseHangfireDashboard() / app.UseHangfireServer().
    /// </summary>
    public static void AddIdentityRecurringJobs()
    {
        // Every 6 months on the 1st day at midnight
        RecurringJob.AddOrUpdate<AccessReviewSchedulerJob>(
            recurringJobId: "identity.access-review-scheduler",
            methodCall: j => j.ExecuteAsync(),
            cronExpression: "0 0 1 */6 *");
    }
}
