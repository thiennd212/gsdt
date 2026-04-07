using Hangfire;

namespace GSDT.Audit.Infrastructure;

/// <summary>Registers Audit Infrastructure services: DbContext, repositories, services.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddAuditInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "audit"));

            options.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ISecurityIncidentRepository, SecurityIncidentRepository>();
        services.AddScoped<IRtbfRequestRepository, RtbfRequestRepository>();

        // M15 AI Governance repositories
        services.AddScoped<IAiPromptTraceRepository, AiPromptTraceRepository>();
        services.AddScoped<IAiOutputReviewRepository, AiOutputReviewRepository>();
        services.AddScoped<ICompliancePolicyRepository, CompliancePolicyRepository>();

        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditLogger, AuditService>();
        // Register HmacChainService for both direct use (Hangfire job) and IHmacChainVerifier (MediatR handler)
        services.AddScoped<HmacChainService>();
        services.AddScoped<IHmacChainVerifier>(sp => sp.GetRequiredService<HmacChainService>());
        services.AddSingleton<IBackgroundAuditJobEnqueuer, HangfireAuditJobEnqueuer>();

        // RTBF PII anonymizer (collected by ProcessRtbfRequestCommandHandler via IEnumerable<IModulePiiAnonymizer>)
        services.AddScoped<IModulePiiAnonymizer, AuditPiiAnonymizer>();

        // M15 AI Governance services
        services.AddScoped<IPiiDetectionService, PiiDetectionService>();
        services.AddScoped<IAiPromptTracer, AiPromptTraceDecorator>();

        // SLA breach checker — registered as scoped; Hangfire resolves from DI
        services.AddScoped<RtbfSlaBreachCheckerJob>();

        return services;
    }

    /// <summary>
    /// Registers Hangfire recurring jobs for the Audit module.
    /// Call from the host after UseHangfireDashboard() / app.UseHangfireServer().
    /// </summary>
    public static void AddAuditRecurringJobs()
    {
        // Daily at midnight — scan for overdue RTBF requests (Law 91/2025 SLA: 30 days)
        RecurringJob.AddOrUpdate<RtbfSlaBreachCheckerJob>(
            recurringJobId: "audit.rtbf-sla-breach-checker",
            methodCall: j => j.ExecuteAsync(),
            cronExpression: Cron.Daily());
    }
}
