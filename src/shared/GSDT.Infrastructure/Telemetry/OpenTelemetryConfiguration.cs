using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

namespace GSDT.Infrastructure.Telemetry;

/// <summary>
/// OpenTelemetry configuration — traces + metrics via OTLP exporter.
/// Prometheus scraping endpoint /metrics via prometheus-net.AspNetCore (UseHttpMetrics + MapMetrics).
/// ActivitySource "GSDT" for manual spans in command/query handlers.
/// </summary>
public static class OpenTelemetryConfiguration
{
    /// <summary>Shared ActivitySource for manual tracing in handlers and services.</summary>
    public static readonly System.Diagnostics.ActivitySource AqtSource = new("GSDT", "1.0.0");

    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otlpEndpoint = configuration["Otel:Endpoint"] ?? "http://localhost:4317";
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService("GSDT", serviceVersion: "1.0.0")
                .AddAttributes([new("deployment.environment", environment)]))
            .WithTracing(tracing => tracing
                .AddSource("GSDT")
                .AddAspNetCoreInstrumentation(opt => opt.RecordException = true)
                .AddHttpClientInstrumentation()
                // SetDbStatementForText=false — no PII/SQL in traces
                .AddEntityFrameworkCoreInstrumentation(opt => opt.SetDbStatementForText = false)
                .AddOtlpExporter(opt => opt.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opt => opt.Endpoint = new Uri(otlpEndpoint)));

        return services;
    }

    /// <summary>
    /// Enables prometheus-net HTTP metrics middleware.
    /// Call before UseRouting so all routes are captured.
    /// </summary>
    public static IApplicationBuilder UsePrometheusMetrics(this IApplicationBuilder app)
    {
        app.UseHttpMetrics();
        return app;
    }
}
