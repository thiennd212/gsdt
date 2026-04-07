using Minio;
using nClam;

namespace GSDT.Files.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddFilesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core — "files" schema
        services.AddDbContext<FilesDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "files"));

            options.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        // Options
        services.Configure<MinioOptions>(configuration.GetSection(MinioOptions.SectionName));
        services.Configure<ClamAvOptions>(configuration.GetSection(ClamAvOptions.SectionName));
        services.Configure<FilesOptions>(configuration.GetSection(FilesOptions.SectionName));

        // MinIO client (singleton — thread-safe, connection-pooled)
        services.AddSingleton<IMinioClient>(sp =>
        {
            var opts = configuration.GetSection(MinioOptions.SectionName).Get<MinioOptions>()
                       ?? new MinioOptions();
            return new MinioClient()
                .WithEndpoint(opts.Endpoint)
                .WithCredentials(opts.AccessKey, opts.SecretKey)
                .WithSSL(opts.UseSSL)
                .Build();
        });

        // Storage + security services
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<IVirusScanner, ClamAvVirusScanner>();
        services.AddScoped<IFileSecurityService, FileSecurityService>();
        services.AddScoped<IDigitalSignatureService, StubDigitalSignatureService>();

        // Repository
        services.AddScoped<IFileRepository, FileRepository>();

        // Hangfire scan job
        services.AddScoped<IClamAvScanJob, ClamAvScanJob>();

        // M08: Document lifecycle generic repositories
        services.AddScoped<IRepository<FileVersion, Guid>, GenericFilesRepository<FileVersion>>();
        services.AddScoped<IRepository<DocumentTemplate, Guid>, GenericFilesRepository<DocumentTemplate>>();
        services.AddScoped<IRepository<RetentionPolicy, Guid>, GenericFilesRepository<RetentionPolicy>>();
        services.AddScoped<IRepository<RecordLifecycle, Guid>, GenericFilesRepository<RecordLifecycle>>();

        // M08: Scriban template engine + enforcement job
        services.AddScoped<ITemplateEngine, ScribanTemplateEngine>();
        services.AddTransient<RetentionPolicyEnforcementJob>();

        // F-04: Use Clients/InProcessFilesModuleClient (repository-based, domain-aligned)
        services.AddScoped<IFilesModuleClient, InProcessFilesModuleClient>();

        // F-03: ClamAV startup health validation — logs CRITICAL if unreachable in production
        services.AddHostedService<ClamAvStartupHealthCheck>();

        return services;
    }
}

/// <summary>
/// F-03: Startup hosted service that validates ClamAV reachability.
/// In production (BypassWhenUnavailable=false), logs a CRITICAL warning if ClamAV is unreachable.
/// This is a warning only — does not block startup — to avoid cascading failures on transient network issues.
/// </summary>
internal sealed class ClamAvStartupHealthCheck(
    IOptions<ClamAvOptions> options,
    ILogger<ClamAvStartupHealthCheck> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var opts = options.Value;

        // Only warn in production mode (bypass disabled = production)
        if (opts.BypassWhenUnavailable)
        {
            logger.LogWarning(
                "ClamAV bypass mode is ACTIVE (BypassWhenUnavailable=true). " +
                "Files will NOT be virus-scanned. Ensure this is disabled in production.");
            return;
        }

        try
        {
            var client = new ClamClient(opts.Host, opts.Port);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var pingResult = await client.PingAsync(cts.Token);
            if (pingResult)
            {
                logger.LogInformation("ClamAV health check passed. Host={Host}:{Port}", opts.Host, opts.Port);
            }
            else
            {
                logger.LogCritical(
                    "SECURITY WARNING: ClamAV is unreachable at {Host}:{Port}. " +
                    "Virus scanning will FAIL for all uploaded files. Investigate immediately.",
                    opts.Host, opts.Port);
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "SECURITY WARNING: ClamAV startup health check failed. Host={Host}:{Port}. " +
                "Virus scanning may be unavailable. Investigate immediately.",
                opts.Host, opts.Port);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
