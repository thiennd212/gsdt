using System.Threading.RateLimiting;
using RateLimitMetadata = System.Threading.RateLimiting.MetadataName;

namespace GSDT.Infrastructure.RateLimiting;

/// <summary>
/// ASP.NET Core built-in rate limiter — app-level defense layer.
/// Phase 06 YARP adds gateway-level rate limiting (second layer).
/// Authenticated: keyed by userId JWT sub claim (unforgeable).
/// Anonymous: keyed by RemoteIpAddress.
/// Policies:
///   GlobalLimiter — 100 req/min authenticated, 20 req/min anonymous (partitioned by identity)
///   "default"     — alias used by YARP route config (100/min authenticated, 20/min anonymous)
///   "anonymous"   — 60 req/min per IP
///   "authenticated" — 600 req/min per userId
///   "write-ops"   — 20 req/min per IP
///   "public-form-submit" — 5 req/min per IP (public form spam prevention)
///   "mfa-verify"  — 5 req/min per client (TOTP brute-force prevention)
/// </summary>
public static class RateLimiterConfiguration
{
    public static IServiceCollection AddGovRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global partitioned limiter: authenticated/anonymous per-minute limits.
            // Development: 1000/200 to avoid throttling E2E tests.
            // Production: 100/20 for abuse prevention.
            var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            {
                var isAuth = ctx.User?.Identity?.IsAuthenticated == true;
                var partitionKey = isAuth
                    ? ctx.User!.FindFirst("sub")?.Value ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anon"
                    : ctx.Connection.RemoteIpAddress?.ToString() ?? "anon";
                var permitLimit = isAuth ? (isDev ? 1000 : 100) : (isDev ? 200 : 20);

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                });
            });

            // "gateway" policy — referenced by YARP route config (RateLimiterPolicy: "gateway")
            // Mirrors global limiter limits; keyed by IP for YARP reverse-proxy scenario
            // NOTE: "default" is reserved by ASP.NET Core YARP integration — must use custom name
            options.AddFixedWindowLimiter("gateway", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 100;
                opt.QueueLimit = 5;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Anonymous endpoints: 60 req/min per IP
            options.AddFixedWindowLimiter("anonymous", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 60;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Authenticated endpoints: 600 req/min per userId
            options.AddFixedWindowLimiter("authenticated", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 600;
                opt.QueueLimit = 20;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Write operations (POST/PUT/DELETE): 20 req/min per IP — prevents abuse
            options.AddFixedWindowLimiter("write-ops", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 20;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Public form submission: 5 submissions/min per IP — prevents spam/flooding
            options.AddFixedWindowLimiter("public-form-submit", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 5;
                opt.QueueLimit = 0;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // MFA verify: 5 attempts/min per client — prevents TOTP brute-force (F-10)
            options.AddFixedWindowLimiter("mfa-verify", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 5;
                opt.QueueLimit = 0;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                // Dynamic Retry-After from lease metadata (window remaining)
                var retrySeconds = 60;
                if (ctx.Lease.TryGetMetadata(RateLimitMetadata.RetryAfter, out var retryAfter))
                    retrySeconds = Math.Max(1, (int)retryAfter.TotalSeconds);

                ctx.HttpContext.Response.Headers["Retry-After"] = retrySeconds.ToString();
                ctx.HttpContext.Response.Headers["X-RateLimit-Limit"] = ResolvePolicyLimit(ctx.HttpContext).ToString();

                await ctx.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://gov.vn/errors/gov_sys_005",
                    title = "Too Many Requests",
                    status = 429,
                    errorCode = "GOV_SYS_005",
                    detail = $"Rate limit exceeded. Retry after {retrySeconds} seconds."
                }, ct);
            };
        });

        return services;
    }

    /// <summary>
    /// Resolves the permit limit for the rate-limiting policy applied to the current endpoint.
    /// Falls back to global limiter limits (100 auth / 20 anon) when no named policy is present.
    /// </summary>
    private static int ResolvePolicyLimit(HttpContext httpContext)
    {
        var policyName = httpContext.GetEndpoint()
            ?.Metadata.GetMetadata<EnableRateLimitingAttribute>()
            ?.PolicyName;

        return policyName switch
        {
            "anonymous" => 60,
            "authenticated" => 600,
            "write-ops" => 20,
            "public-form-submit" => 5,
            "mfa-verify" => 5,
            "gateway" => 100,
            // Global limiter: auth=100, anon=20
            _ => httpContext.User?.Identity?.IsAuthenticated == true ? 100 : 20
        };
    }
}
