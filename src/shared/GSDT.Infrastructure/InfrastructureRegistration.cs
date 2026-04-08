using StackExchange.Redis;

namespace GSDT.Infrastructure;

/// <summary>
/// Cross-cutting infrastructure DI registration — called once in Program.cs.
/// Registers: logging, telemetry, caching, rate limiting, health checks, CORS,
/// response compression, EF interceptors, outbox processor, circuit breaker registry.
/// Module-specific infra registered in each module's own InfrastructureRegistration.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- Options ---
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.PostConfigure<DatabaseOptions>(opts =>
        {
            if (string.IsNullOrEmpty(opts.ConnectionString))
                opts.ConnectionString = configuration.GetConnectionString("Default") ?? string.Empty;
        });
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        // --- Core plumbing ---
        services.AddHttpContextAccessor();
        // ICurrentUser / ITenantContext — resolved from JWT claims in active HTTP request
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        // ITenantContext: use HttpContext when available (web requests),
        // fall back to BackgroundJobTenantContext for Hangfire jobs (no HttpContext)
        services.AddScoped<ITenantContext>(sp =>
        {
            var httpAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return httpAccessor.HttpContext is not null
                ? new HttpContextTenantContext(httpAccessor)
                : new BackgroundJobs.BackgroundJobTenantContext();
        });
        services.AddScoped<IReadDbConnection, DapperReadDbConnection>();
        services.AddScoped<IDomainEventPublisher, Events.MediatRDomainEventPublisher>();

        // --- Export (IExcelExporter, IPdfExporter) ---
        services.AddExport();

        // --- EF interceptors ---
        services.AddSingleton<SlowQueryInterceptor>();
        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddSingleton<OutboxInterceptor>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<TenantSessionContextInterceptor>();

        // Outbox processor stub — Phase 02c MassTransit provides production impl
        services.AddHostedService<OutboxProcessor>();

        // --- Redis + two-tier cache ---
        var redisConnStr = configuration["Redis:ConnectionString"];
        if (string.IsNullOrEmpty(redisConnStr)) redisConnStr = "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var config = ConfigurationOptions.Parse(redisConnStr);
            config.AbortOnConnectFail = false; // C-05: app starts even if Redis is down
            config.ConnectRetry = 3;
            config.ConnectTimeout = 5000;
            return ConnectionMultiplexer.Connect(config);
        });

        services.AddMemoryCache(opt =>
            opt.SizeLimit = configuration.GetValue<int>("Cache:L1:MaxEntries", 5000));

        services.AddSingleton<ICacheService, TwoTierCacheService>();

        // --- IDistributedCache backed by Redis (used by Forms ExternalDataSourceResolver, AI CachedEmbedding) ---
        services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConnStr);

        // --- Resilience ---
        services.AddSingleton<ICircuitBreakerRegistry, InMemoryCircuitBreakerRegistry>();

        // --- Outbound HttpClient propagation ---
        services.AddTransient<TenantPropagationHandler>();

        // --- Rate limiter (app-level; YARP adds gateway-level in Phase 06) ---
        services.AddGovRateLimiter();

        // --- Health checks ---
        services.AddGovHealthChecks(configuration);

        // --- OpenTelemetry (traces + metrics via OTLP) ---
        services.AddOpenTelemetryObservability(configuration);

        // --- i18n ---
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(opts =>
        {
            var supported = new[] { "vi", "en" };
            opts.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("vi");
            opts.SupportedCultures = supported
                .Select(c => new System.Globalization.CultureInfo(c)).ToList();
            opts.SupportedUICultures = opts.SupportedCultures;
        });

        // --- CORS ---
        services.AddCors(opts =>
        {
            opts.AddPolicy("GovApiPolicy", policy =>
            {
                var origins = configuration
                    .GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? ["http://localhost:3000", "http://localhost:4200", "http://localhost:5173"];
                policy.WithOrigins(origins)
                    .WithHeaders("Content-Type", "Authorization", "X-Api-Key", "X-XSRF-TOKEN", "Accept", "Accept-Language")
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                    .AllowCredentials();
            });
        });

        // --- Antiforgery (X-XSRF-TOKEN header) ---
        services.AddAntiforgery(opts => opts.HeaderName = "X-XSRF-TOKEN");

        // --- Response compression (Brotli + Gzip) ---
        services.AddResponseCompression(opts =>
        {
            opts.EnableForHttps = true;
            opts.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults
                .MimeTypes
                .Concat(["application/json", "application/problem+json"]);
        });

        // --- Background jobs (Hangfire + NĐ53 archive) ---
        services.AddHangfireJobs(configuration);

        // --- Search (SQL FTS default; switch via Search:Provider = "Elasticsearch") ---
        services.AddSearchInfrastructure(configuration);

        // --- Extension Framework (M17) ---
        services.AddExtensionFramework();

        // --- Graceful shutdown ---
        services.Configure<HostOptions>(opts =>
            opts.ShutdownTimeout = TimeSpan.FromSeconds(30));

        return services;
    }
}
