
namespace GSDT.Infrastructure.Webhooks;

/// <summary>
/// DI registration for the webhook engine.
/// Call services.AddWebhookEngine(configuration) in Program.cs after AddInfrastructure().
/// </summary>
public static class WebhookRegistration
{
    public static IServiceCollection AddWebhookEngine(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Dedicated DbContext for webhook schema
        var connStr = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default is required for WebhookDbContext.");

        services.AddDbContext<WebhookDbContext>(opts =>
            opts.UseSqlServer(connStr, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "webhooks")));

        // SSRF validator — singleton (stateless, DNS calls are cheap to recreate)
        services.AddSingleton<WebhookUrlValidator>();

        // Signing service — static methods only; no registration needed

        // Delivery job — Scoped (resolves WebhookDbContext per job execution)
        services.AddScoped<WebhookDeliveryJob>();

        // Dispatcher — Scoped (resolves DbContext + IBackgroundJobClient per request)
        services.AddScoped<IWebhookService, WebhookDispatcher>();

        // Named HttpClient for webhook delivery (10s timeout enforced in WebhookDeliveryJob)
        services.AddHttpClient("webhook", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("GSDT-Webhook/1.0");
        });

        return services;
    }
}
