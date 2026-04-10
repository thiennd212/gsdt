namespace GSDT.Identity.Application.Authorization;

/// <summary>
/// Static seed data for permission records and role-permission assignments.
/// Extracted from seeder for testability. Used by GsdtPermissionSeeder at startup.
///
/// Matrix matches current [Authorize(Roles)] on all 6 investment controllers:
///   READ:  Admin, BTC, CQCQ, CDT
///   WRITE: Admin, BTC, CDT (CQCQ excluded)
///   DELETE: Admin, BTC, CDT (CQCQ excluded)
/// </summary>
public static class PermissionSeedDefinitions
{
    /// <summary>
    /// Permission seed record: (Code, Name, ModuleCode, ResourceCode, ActionCode, IsSensitive).
    /// </summary>
    public sealed record PermissionSeed(
        string Code, string Name, string ModuleCode, string ResourceCode, string ActionCode, bool IsSensitive);

    // ── Investment permissions (6 types × 3 actions = 18) ────────────────────
    private static readonly PermissionSeed[] InvestmentPermissions =
    [
        new(Permissions.Inv.DomesticRead,  "Xem du an trong nuoc",      "INV", "DOMESTIC", "READ",   false),
        new(Permissions.Inv.DomesticWrite, "Tao/sua du an trong nuoc",  "INV", "DOMESTIC", "WRITE",  false),
        new(Permissions.Inv.DomesticDelete,"Xoa du an trong nuoc",      "INV", "DOMESTIC", "DELETE", true),
        new(Permissions.Inv.OdaRead,       "Xem du an ODA",             "INV", "ODA",      "READ",   false),
        new(Permissions.Inv.OdaWrite,      "Tao/sua du an ODA",         "INV", "ODA",      "WRITE",  false),
        new(Permissions.Inv.OdaDelete,     "Xoa du an ODA",             "INV", "ODA",      "DELETE", true),
        new(Permissions.Inv.PppRead,       "Xem du an PPP",             "INV", "PPP",      "READ",   false),
        new(Permissions.Inv.PppWrite,      "Tao/sua du an PPP",         "INV", "PPP",      "WRITE",  false),
        new(Permissions.Inv.PppDelete,     "Xoa du an PPP",             "INV", "PPP",      "DELETE", true),
        new(Permissions.Inv.DnnnRead,      "Xem du an DNNN",            "INV", "DNNN",     "READ",   false),
        new(Permissions.Inv.DnnnWrite,     "Tao/sua du an DNNN",        "INV", "DNNN",     "WRITE",  false),
        new(Permissions.Inv.DnnnDelete,    "Xoa du an DNNN",            "INV", "DNNN",     "DELETE", true),
        new(Permissions.Inv.NdtRead,       "Xem du an NDT",             "INV", "NDT",      "READ",   false),
        new(Permissions.Inv.NdtWrite,      "Tao/sua du an NDT",         "INV", "NDT",      "WRITE",  false),
        new(Permissions.Inv.NdtDelete,     "Xoa du an NDT",             "INV", "NDT",      "DELETE", true),
        new(Permissions.Inv.FdiRead,       "Xem du an FDI",             "INV", "FDI",      "READ",   false),
        new(Permissions.Inv.FdiWrite,      "Tao/sua du an FDI",         "INV", "FDI",      "WRITE",  false),
        new(Permissions.Inv.FdiDelete,     "Xoa du an FDI",             "INV", "FDI",      "DELETE", true),
    ];

    // ── Admin permissions (5) ────────────────────────────────────────────────
    private static readonly PermissionSeed[] AdminPermissions =
    [
        new(Permissions.Admin.RoleRead,   "Xem vai tro",            "ADMIN", "ROLE", "READ",   false),
        new(Permissions.Admin.RoleWrite,  "Tao/sua vai tro",        "ADMIN", "ROLE", "WRITE",  false),
        new(Permissions.Admin.RoleDelete, "Xoa vai tro",            "ADMIN", "ROLE", "DELETE", true),
        new(Permissions.Admin.PermRead,   "Xem quyen",              "ADMIN", "PERM", "READ",   false),
        new(Permissions.Admin.PermAssign, "Gan quyen cho vai tro",  "ADMIN", "PERM", "ASSIGN", true),
    ];

    /// <summary>All 23 permissions to seed.</summary>
    public static IReadOnlyList<PermissionSeed> AllPermissions { get; } =
        InvestmentPermissions.Concat(AdminPermissions).ToArray();

    /// <summary>
    /// Role → permission codes mapping. Matches current [Authorize(Roles)] matrix exactly.
    /// BTC: all 18 investment | CQCQ: 6 READ only | CDT: all 18 investment | Admin: all 23.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> RolePermissionMap { get; } =
        new Dictionary<string, string[]>
        {
            ["BTC"] = InvestmentPermissions.Select(p => p.Code).ToArray(),
            ["CDT"] = InvestmentPermissions.Select(p => p.Code).ToArray(),
            ["CQCQ"] = InvestmentPermissions.Where(p => p.ActionCode == "READ").Select(p => p.Code).ToArray(),
            ["Admin"] = InvestmentPermissions.Concat(AdminPermissions).Select(p => p.Code).ToArray(),
            ["SystemAdmin"] = InvestmentPermissions.Concat(AdminPermissions).Select(p => p.Code).ToArray(),
        };
}
