using Hangfire;
using MediatR;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// === Vault secrets (Production only — loads from HashiCorp Vault KV v2) ===
builder.Configuration.AddVaultConfiguration(opts =>
{
    var section = builder.Configuration.GetSection(VaultOptions.SectionName);
    section.Bind(opts);
    // Env vars override appsettings for Vault connection itself
    var addr = Environment.GetEnvironmentVariable("VAULT_ADDR");
    if (!string.IsNullOrEmpty(addr)) opts.Address = addr;
    opts.Token = Environment.GetEnvironmentVariable("VAULT_TOKEN") ?? opts.Token;
    opts.RoleId = Environment.GetEnvironmentVariable("VAULT_ROLE_ID") ?? opts.RoleId;
    opts.SecretId = Environment.GetEnvironmentVariable("VAULT_SECRET_ID") ?? opts.SecretId;
    var path = Environment.GetEnvironmentVariable("VAULT_SECRET_PATH");
    if (!string.IsNullOrEmpty(path)) opts.SecretPath = path;
});

// L-02: Global request body size limit — prevents DoS via oversized uploads on any endpoint
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104_857_600; // 100 MB
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
});

// === Logging (Serilog structured JSON — must be first) ===
builder.AddSerilogLogging();

// === Core Services ===
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Serialize enums as strings for REST API readability and FE compatibility
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .AddApplicationPart(
        typeof(GSDT.Notifications.Presentation.Controllers.NotificationsController).Assembly)
    .AddApplicationPart(
        typeof(GSDT.Audit.Presentation.Controllers.AuditLogsController).Assembly)
    .AddApplicationPart(typeof(GSDT.Api.Controllers.Admin.DeadLettersAdminController).Assembly)
    .AddApplicationPart(typeof(GSDT.Api.Controllers.Admin.WebhooksAdminController).Assembly)
    .AddApplicationPart(typeof(GSDT.Api.Controllers.Admin.ApiKeysAdminController).Assembly)
    .AddApplicationPart(typeof(GSDT.Api.Controllers.Admin.EventCatalogAdminController).Assembly)
    .AddApplicationPart(typeof(GSDT.Api.Controllers.Admin.AlertingAdminController).Assembly)
    .AddApplicationPart(
        typeof(GSDT.Identity.Presentation.Controllers.UsersAdminController).Assembly)
    .AddApplicationPart(
        typeof(GSDT.MasterData.Presentation.Controllers.MasterDataController).Assembly)
    .AddApplicationPart(
        typeof(GSDT.SystemParams.Presentation.Controllers.SystemParamsController).Assembly)
    .AddApplicationPart(
        typeof(GSDT.Organization.Presentation.Controllers.OrgUnitsController).Assembly)
    .AddApplicationPart(
        typeof(GSDT.Files.Presentation.Controllers.FilesController).Assembly)
    .AddApplicationPart(
        typeof(GSDT.InvestmentProjects.Presentation.Controllers.DomesticProjectsController).Assembly);

// === Gateway (YARP + API Versioning + OpenAPI/Scalar) ===
builder.Services.AddGateway(builder.Configuration);

// === Presentation-layer validators (non-CQRS controllers) ===
FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssemblyContaining(
    builder.Services,
    typeof(GSDT.SystemParams.Presentation.Controllers.AnnouncementsController),
    ServiceLifetime.Scoped);

// === API Key Auth (M2M — SHA-256 + Redis cache) ===
builder.Services.AddApiKeyAuth(builder.Configuration);

// === MediatR Pipeline (order: TenantAware first, then Validation) ===
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TenantAwareBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
});

// === SharedKernel.Contracts defaults (monolith mode) ===
builder.Services.AddSingleton<IServiceIdentityProvider, NoOpServiceIdentityProvider>();

// === Infrastructure (cross-cutting: cache, rate limit, OTEL, health checks, CORS, etc.) ===
builder.Services.AddInfrastructure(builder.Configuration);

// === Event Catalog (M07 — in-memory registry of domain events) ===
builder.Services.AddSingleton<IEventCatalogService, EventCatalogService>();

// === Webhook Engine (outgoing HTTP dispatcher + subscription admin) ===
builder.Services.AddWebhookEngine(builder.Configuration);

// === Backup/Restore infrastructure (NĐ53 compliance) ===
builder.Services.AddBackupInfrastructure(builder.Configuration);

// === Alerting infrastructure (M14 — AlertRules, Runbooks, evaluation job) ===
builder.Services.AddAlertingInfrastructure(builder.Configuration);

// === Message Bus (MassTransit — InMemory dev / RabbitMQ prod) ===
builder.Services.AddMessageBus(builder.Configuration);

// === Dead Letter admin service ===
builder.Services.AddScoped<DeadLetterService>();

// === Module Registration ===
builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddIdentityInfrastructure(builder.Configuration, builder.Environment);

builder.Services
    .AddNotificationsModule(builder.Configuration)
    .AddNotificationsInfrastructure(builder.Configuration);

// === SignalR Redis backplane (must follow AddNotificationsInfrastructure which calls AddSignalR) ===
var redisConn = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrEmpty(redisConn))
    builder.Services.AddSignalR().AddStackExchangeRedis(redisConn);

builder.Services
    .AddAuditModule(builder.Configuration)
    .AddAuditInfrastructure(builder.Configuration);

builder.Services.AddMasterData(builder.Configuration);
builder.Services.AddSystemParams(builder.Configuration);
builder.Services.AddOrganizationModule(builder.Configuration);

// === E2E test data seeder (Development only) ===
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<GSDT.Api.E2ETestDataSeeder>();
    // Seeds GSDT roles (BTC, CQCQ, CDT) + test users for development
    builder.Services.AddHostedService<GSDT.Api.GsdtRoleSeeder>();
}

builder.Services
    .AddFilesModule(builder.Configuration)
    .AddFilesInfrastructure(builder.Configuration);

builder.Services.AddInvestmentProjects(builder.Configuration);

builder.Services
    .AddIntegrationModule()
    .AddIntegrationInfrastructure(builder.Configuration);

// === Multi-scheme auth policy (ApiKey for M2M, Bearer/OpenIddict for user tokens) ===
// After all modules are registered — schemes must exist before the policy references them.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiAuth";
    options.DefaultAuthenticateScheme = "MultiAuth";
    options.DefaultChallengeScheme = "MultiAuth";
})
.AddPolicyScheme("MultiAuth", "MultiAuth", options =>
{
    options.ForwardDefaultSelector = ctx =>
        ctx.Request.Headers.ContainsKey("X-Api-Key") || ctx.Request.Query.ContainsKey("api_key")
            ? "ApiKey"
            : "OpenIddict.Validation.AspNetCore";
});

// === DI Validation (dev + staging — catches misconfigured services at startup) ===
if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
{
    builder.Host.UseDefaultServiceProvider(opts =>
    {
        opts.ValidateScopes = true;
        opts.ValidateOnBuild = true;
    });
}

// === Output caching for read endpoints (p95 optimization) ===
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.NoCache());
    options.AddPolicy("CasesList", policy =>
        policy.Expire(TimeSpan.FromSeconds(30))
              .SetVaryByQuery("page", "pageSize", "status", "type")
              .SetVaryByHeader("Authorization"));
    options.AddPolicy("MasterData", policy =>
        policy.Expire(TimeSpan.FromMinutes(5))
              .SetVaryByQuery("type"));
    options.AddPolicy("ReportDashboard", policy =>
        policy.Expire(TimeSpan.FromMinutes(5))
              .SetVaryByHeader("Authorization"));
});

var app = builder.Build();

// === M07: Register known domain events in EventCatalogService ===
// Singleton resolved directly — no scope needed for in-memory registration.
{
    var catalog = app.Services.GetRequiredService<IEventCatalogService>();
}

// === Run EF migrations on startup (idempotent — safe to run every boot) ===
// Runs in all environments including Testing — Testcontainers starts a fresh SQL Server per run.
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    await sp.GetRequiredService<ApiKeyDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<IdentityDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<AuditDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<FilesDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<MasterDataDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<NotificationsDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<OrgDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<SystemParamsDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<WebhookDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<BackupDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<AlertingDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<IntegrationDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<InvestmentProjectsDbContext>().Database.MigrateAsync();

    // === Connection pool warming (p95 optimization) ===
    // Opens+returns connections so first real request doesn't pay cold-start penalty
    try
    {
        await sp.GetRequiredService<IdentityDbContext>().Database.CanConnectAsync();
        await sp.GetRequiredService<FilesDbContext>().Database.CanConnectAsync();
    }
    catch { /* Non-fatal — pool warms on first request instead */ }

    // Force-resolve Hangfire storage singleton from DI so JobStorage.Current is set.
    // Previously UseHangfireDashboard() did this implicitly, but it was moved after
    // UseAuthentication() for security — so we need explicit resolution here.
    _ = app.Services.GetRequiredService<Hangfire.JobStorage>();

    // Migrate orphaned jobs from old colon-delimited queue names (idempotent)
    HangfireRegistration.MigrateHangfireQueues(
        builder.Configuration,
        app.Services.GetService<ILoggerFactory>()?.CreateLogger("HangfireMigration"));

    // Register NĐ53 recurring archive jobs (idempotent — safe on every startup)
    HangfireRegistration.RegisterRecurringJobs();

    // Reporting: nightly report file cleanup (E-06)

    // Workflow: SLA breach checker — every 5 minutes
    // M08: Retention policy enforcement — daily at 2 AM UTC
    RecurringJob.AddOrUpdate<GSDT.Files.Infrastructure.Jobs.RetentionPolicyEnforcementJob>(
        "files-retention-policy-enforcement",
        job => job.ExecuteAsync(CancellationToken.None),
        "0 2 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    // M14: Alert evaluation — every 1 minute
    AlertingRegistration.RegisterRecurringJobs();
}

// === Middleware Pipeline (ORDER IS CRITICAL) ===

// 1. Response compression — outermost so all responses are compressed
app.UseResponseCompression();

// 2. Security headers — before any content is written
app.UseSecurityHeaders();

// 3. Correlation ID — enrich all subsequent logs
app.UseCorrelationId();

// 4. Global exception handler — catch all unhandled exceptions
app.UseGlobalExceptionHandler();

// 5. IP filter — block before expensive processing
app.UseIpFilter();

// 6. Prometheus HTTP metrics middleware
app.UsePrometheusMetrics();

// 7. Rate limiting
app.UseRateLimiter();

// 8. CORS
app.UseCors("GovApiPolicy");

// 9. Cookie policy
app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict
});

// 10. Request localization (vi default)
app.UseRequestLocalization();

// HTTPS always enforced — disable only via explicit config flag for Docker dev
if (builder.Configuration.GetValue("ForceHttps", true))
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// === Hangfire dashboard — MUST be after UseAuthentication so HttpContext.User is populated ===
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()],
    DisplayStorageConnectionString = false
});

// QĐ742 1.3c/d/đ: Password expiry enforcement — returns 403 + X-Password-Expired header
app.UseMiddleware<GSDT.Identity.Infrastructure.Middleware.PasswordExpiryMiddleware>();

// Gateway: OpenAPI + Scalar UI + YARP reverse proxy
app.UseGateway();

// 11. Output caching — after auth so cached responses respect authorization
app.UseOutputCache();

// 12. Idempotency — after auth so tenantId claim is available
app.UseIdempotency();

// 13. Anti double-submit (X-Request-Token, 5s TTL) — complements idempotency for UI race conditions
app.UseAntiDoubleSubmit();

app.MapControllers();

// SignalR hubs — authenticated connections only
app.MapHub<NotificationsHub>("/hubs/notifications");

// Health check endpoints
app.MapGovHealthChecks();

// Prometheus scraping endpoint
app.MapMetrics("/metrics");

app.Run();

// Expose Program for integration test WebApplicationFactory
public partial class Program { }
