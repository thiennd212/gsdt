using Serilog;
using Serilog.Formatting.Json;

namespace GSDT.Infrastructure.Logging;

/// <summary>
/// Configures Serilog structured logging.
/// Console JSON + daily rolling file. Production level: Warning (configured via appsettings).
/// PII masking applied via LogSanitizingDestructuringPolicy (Phase 05).
/// </summary>
public static class SerilogConfiguration
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, services, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "GSDT")
            .Enrich.WithProperty(
                "Version",
                typeof(SerilogConfiguration).Assembly.GetName().Version?.ToString() ?? "unknown")
            .WriteTo.Console(new JsonFormatter(renderMessage: true))
            .WriteTo.File(
                new JsonFormatter(),
                path: "logs/gsdt-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 100_000_000));

        return builder;
    }
}
