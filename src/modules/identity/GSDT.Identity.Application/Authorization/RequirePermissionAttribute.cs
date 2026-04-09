namespace GSDT.Identity.Application.Authorization;

/// <summary>
/// Requires the caller to hold the specified permission code.
/// Translates to Policy = "PERM:{permissionCode}" which is resolved
/// by <see cref="PermissionPolicyProvider"/> at runtime.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = $"{PermissionPolicyProvider.PolicyPrefix}{permissionCode}";
    }
}
