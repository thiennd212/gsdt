
namespace GSDT.Infrastructure.HealthChecks;

/// <summary>Lightweight HTTP-GET health check used for AI service probes (Qdrant, Ollama).</summary>
internal sealed class HttpGetHealthCheck(string url, TimeSpan timeout) : IHealthCheck
{
    private static readonly HttpClient SharedClient = new();

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            var response = await SharedClient.GetAsync(url, cts.Token);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Degraded($"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(ex.Message);
        }
    }
}

/// <summary>
/// Health check registration and endpoint mapping.
/// /health/live  — K8s liveness probe (no dependency checks, pod is running).
/// /health/ready — K8s readiness probe (SQL Server + Redis connectivity).
/// </summary>
public static class HealthCheckConfiguration
{
    public static IServiceCollection AddGovHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("Default")
            ?? configuration["Database:ConnectionString"]
            ?? string.Empty;
        var redisConnStr = configuration["Redis:ConnectionString"] ?? "localhost:6379";

        var checks = services.AddHealthChecks()
            .AddSqlServer(
                connectionString: connStr,
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "ready"])
            .AddRedis(
                redisConnectionString: redisConnStr,
                name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: ["cache", "ready"]);

        // AI infrastructure health checks — only when Ollama is enabled
        var ollamaEnabled = configuration.GetSection("AI:Ollama").GetValue<bool>("Enabled", false);
        if (ollamaEnabled)
        {
            var qdrantBaseUrl = configuration["AI:Qdrant:Endpoint"] ?? "http://localhost:6333";
            var ollamaBaseUrl = configuration["AI:Ollama:Endpoint"] ?? "http://localhost:11434";
            var timeout = TimeSpan.FromSeconds(3);

            checks
                .AddCheck("qdrant",
                    new HttpGetHealthCheck(qdrantBaseUrl + "/readyz", timeout),
                    failureStatus: HealthStatus.Degraded,
                    tags: ["ready"])
                .AddCheck("ollama",
                    new HttpGetHealthCheck(ollamaBaseUrl + "/api/tags", timeout),
                    failureStatus: HealthStatus.Degraded,
                    tags: ["ready"]);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapGovHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Liveness: always healthy — pod is alive
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteJsonResponse
        });

        // Readiness: external dependency checks
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Contains("ready"),
            ResponseWriter = WriteJsonResponse
        });

        return endpoints;
    }

    private static async Task WriteJsonResponse(HttpContext ctx, HealthReport report)
    {
        ctx.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = (int)e.Value.Duration.TotalMilliseconds
            }),
            totalDurationMs = (int)report.TotalDuration.TotalMilliseconds
        };
        await ctx.Response.WriteAsJsonAsync(response);
    }
}
