using Hangfire.Dashboard;

namespace GSDT.Infrastructure.BackgroundJobs;

/// <summary>
/// Restricts Hangfire dashboard to SystemAdmin role only.
/// DisplayStorageConnectionString = false prevents connection string leakage.
/// Dev: allows unauthenticated access. Prod: requires SystemAdmin role claim.
/// </summary>
public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow unauthenticated access in Development environment only
        var env = httpContext.RequestServices.GetService<IHostEnvironment>();
        if (env?.IsDevelopment() == true)
            return true;

        return httpContext.User?.IsInRole("SystemAdmin") == true;
    }
}
