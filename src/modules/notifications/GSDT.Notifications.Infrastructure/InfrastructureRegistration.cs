
namespace GSDT.Notifications.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<NotificationsDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "notifications"));

            options.AddInterceptors(
                sp.GetRequiredService<SlowQueryInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantSessionContextInterceptor>());
        });

        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();

        // Email (MailKit SMTP)
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailSender, MailKitEmailSender>();

        // SMS (HTTP webhook)
        services.Configure<SmsWebhookOptions>(configuration.GetSection(SmsWebhookOptions.SectionName));
        services.AddHttpClient("SmsWebhook", (sp, client) =>
        {
            var opts = configuration.GetSection(SmsWebhookOptions.SectionName).Get<SmsWebhookOptions>();
            if (opts is not null && !string.IsNullOrEmpty(opts.BearerToken))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opts.BearerToken);
        });
        services.AddScoped<ISmsProvider, WebhookSmsProvider>();

        // Scriban template renderer
        services.AddScoped<ITemplateRenderer, ScribanTemplateRenderer>();

        // SignalR (Redis backplane added in Program.cs where Redis config is available)
        services.AddSignalR();

        // Seed default notification templates (idempotent — GOV-branded HTML email + InApp + SMS)
        services.AddHostedService<NotificationTemplateSeeder>();

        return services;
    }
}
