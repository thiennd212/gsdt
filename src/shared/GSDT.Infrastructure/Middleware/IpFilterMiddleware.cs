using System.Net;

namespace GSDT.Infrastructure.Middleware;

/// <summary>
/// IP Allowlist/Blocklist middleware.
/// Rules loaded from ICacheService (refreshed from DB every 5 min — Phase 02 Admin API).
/// Block rules applied here; allow-list override is handled by loading empty block set.
/// </summary>
public sealed class IpFilterMiddleware(
    RequestDelegate next,
    ICacheService cache,
    ILogger<IpFilterMiddleware> logger)
{
    private const string RulesCacheKey = "system:ip-rules";

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;

        if (remoteIp is null || IPAddress.IsLoopback(remoteIp))
        {
            await next(context);
            return;
        }

        var rules = await cache.GetAsync<IpRuleSet>(RulesCacheKey) ?? IpRuleSet.Empty;

        if (rules.IsBlocked(remoteIp))
        {
            logger.LogWarning("IP blocked by rule: {IpAddress}", remoteIp);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Access denied.",
                errorCode = "GOV_SEC_003"
            });
            return;
        }

        await next(context);
    }
}

/// <summary>Simple IP rule set — loaded from cache, refreshed from DB by admin API.</summary>
public sealed record IpRuleSet(HashSet<string> BlockedIps)
{
    public static readonly IpRuleSet Empty = new([]);

    public bool IsBlocked(IPAddress ip) =>
        BlockedIps.Contains(ip.ToString());
}

public static class IpFilterExtensions
{
    public static IApplicationBuilder UseIpFilter(this IApplicationBuilder app) =>
        app.UseMiddleware<IpFilterMiddleware>();
}
