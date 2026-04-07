namespace GSDT.Identity.Application.Authorization;

/// <summary>
/// Compile-time policy name constants — use with [Authorize(Policy = Policies.Admin)].
/// RBAC roles defined in code (fast path); ABAC attribute rules in DB (dynamic).
/// </summary>
public static class Policies
{
    public const string Admin = nameof(Admin);
    public const string GovOfficer = nameof(GovOfficer);
    public const string Citizen = nameof(Citizen);

    /// <summary>Requires matching DepartmentCode claim — enforced by DepartmentAccessHandler.</summary>
    public const string DepartmentRestricted = nameof(DepartmentRestricted);

    /// <summary>Requires sufficient ClearanceLevel — enforced by ClassificationAccessHandler.</summary>
    public const string ClassifiedAccess = nameof(ClassifiedAccess);
}

/// <summary>Default role names — seeded on startup.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string GovOfficer = "GovOfficer";
    public const string Citizen = "Citizen";
    public const string SystemAdmin = "SystemAdmin";
}
