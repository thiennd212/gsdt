using MassTransit;

namespace GSDT.Infrastructure.Messaging;

/// <summary>
/// DI extension for MassTransit message bus.
/// Call services.AddMessageBus(config) in Program.cs after AddInfrastructure().
/// Transport switch: Bus:Transport = "InMemory" (default/dev) | "RabbitMQ" (staging/prod).
/// </summary>
public static class MessagingRegistration
{
    public static IServiceCollection AddMessageBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? registerConsumers = null)
    {
        // Bind options
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        // MessagingDbContext for dead_letters table
        var connStr = configuration.GetConnectionString("Default")
            ?? configuration["Database:ConnectionString"]
            ?? string.Empty;
        services.AddDbContext<MessagingDbContext>(opts =>
            opts.UseSqlServer(connStr));

        var transport = configuration.GetValue<string>("Bus:Transport") ?? "InMemory";

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // Dead letter consumer — always registered
            x.AddConsumer<DeadLetterConsumer>();

            // Allow modules to register their own consumers
            registerConsumers?.Invoke(x);

            if (transport.Equals("RabbitMQ", StringComparison.OrdinalIgnoreCase))
                ConfigureRabbitMq(x, configuration);
            else
                x.UsingInMemory((ctx, cfg) =>
                {
                    cfg.UseMessageRetry(r =>
                        r.Intervals(
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromMinutes(5)));
                    // H-02: Outgoing messages only sent after consumer succeeds (dedup on retry)
                    cfg.UseInMemoryOutbox(ctx);
                    cfg.ConfigureEndpoints(ctx);
                });
        });

        services.AddScoped<IMessageBus, MassTransitMessageBus>();

        return services;
    }

    private static void ConfigureRabbitMq(
        IBusRegistrationConfigurator x,
        IConfiguration configuration)
    {
        var host = configuration["Bus:RabbitMq:Host"] ?? "localhost";
        var vhost = configuration["Bus:RabbitMq:VHost"] ?? "/";
        var user = configuration["Bus:RabbitMq:Username"] ?? "guest";
        var pass = configuration["Bus:RabbitMq:Password"] ?? "guest";

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(host, vhost, h =>
            {
                h.Username(user);
                h.Password(pass);
            });

            // Exponential backoff — 5s / 30s / 5m (avoids queue overload on transient errors)
            cfg.UseMessageRetry(r =>
                r.Intervals(
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(5)));

            // Prefetch 16 — balanced throughput without memory pressure
            cfg.PrefetchCount = 16;

            // H-02: Outgoing messages only sent after consumer succeeds (dedup on retry)
            cfg.UseInMemoryOutbox(ctx);

            // System.Text.Json serialization
            cfg.UseRawJsonSerializer();

            cfg.ConfigureEndpoints(ctx);
        });
    }
}
