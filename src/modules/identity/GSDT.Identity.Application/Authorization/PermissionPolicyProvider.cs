using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace GSDT.Identity.Application.Authorization;

/// <summary>
/// Dynamic <see cref="IAuthorizationPolicyProvider"/> that intercepts policy names
/// prefixed with "PERM:" and builds an <see cref="AuthorizationPolicy"/> containing
/// a <see cref="PermissionRequirement"/>. Non-PERM policies fall through to the
/// default provider (preserves existing named policies like "Admin", "GovOfficer").
///
/// Registered as Singleton (ASP.NET Core requirement for policy providers).
/// Thread-safe via <see cref="ConcurrentDictionary{TKey,TValue}"/> policy cache (H7).
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public const string PolicyPrefix = "PERM:";

    private readonly DefaultAuthorizationPolicyProvider _fallback;
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _cache = new();

    /// <summary>Deny policy for invalid PERM: codes (C3 fix — avoids InvalidOperationException).</summary>
    private static readonly AuthorizationPolicy DenyPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim("__deny__") // impossible claim — always rejects
        .Build();

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.Ordinal))
            return _fallback.GetPolicyAsync(policyName);

        var code = policyName[PolicyPrefix.Length..];

        // C3: empty code → deny policy (not null) to avoid InvalidOperationException
        if (string.IsNullOrEmpty(code))
            return Task.FromResult<AuthorizationPolicy?>(DenyPolicy);

        // H7: cache built policies — same permission code always returns same instance
        var policy = _cache.GetOrAdd(policyName, _ =>
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(code))
                .Build());

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();
}
