namespace GSDT.Identity.Application.Authorization;

/// <summary>
/// Compile-time permission code constants. Format: MODULE.RESOURCE.ACTION.
/// Use with [RequirePermission(Permissions.Inv.DomesticRead)] for type safety.
/// Seed data in Phase 4 must match these codes exactly.
/// </summary>
public static class Permissions
{
    /// <summary>Investment module permissions — 6 project types × 3 actions.</summary>
    public static class Inv
    {
        // Domestic (Trong nước)
        public const string DomesticRead = "INV.DOMESTIC.READ";
        public const string DomesticWrite = "INV.DOMESTIC.WRITE";
        public const string DomesticDelete = "INV.DOMESTIC.DELETE";

        // ODA
        public const string OdaRead = "INV.ODA.READ";
        public const string OdaWrite = "INV.ODA.WRITE";
        public const string OdaDelete = "INV.ODA.DELETE";

        // PPP
        public const string PppRead = "INV.PPP.READ";
        public const string PppWrite = "INV.PPP.WRITE";
        public const string PppDelete = "INV.PPP.DELETE";

        // DNNN (Doanh nghiệp nhà nước)
        public const string DnnnRead = "INV.DNNN.READ";
        public const string DnnnWrite = "INV.DNNN.WRITE";
        public const string DnnnDelete = "INV.DNNN.DELETE";

        // NDT (Nhà đầu tư)
        public const string NdtRead = "INV.NDT.READ";
        public const string NdtWrite = "INV.NDT.WRITE";
        public const string NdtDelete = "INV.NDT.DELETE";

        // FDI
        public const string FdiRead = "INV.FDI.READ";
        public const string FdiWrite = "INV.FDI.WRITE";
        public const string FdiDelete = "INV.FDI.DELETE";
    }

    /// <summary>MasterData module permissions.</summary>
    public static class MasterData
    {
        public const string Read = "MASTER.DATA.READ";
        public const string Write = "MASTER.DATA.WRITE";
        public const string Delete = "MASTER.DATA.DELETE";
        public const string SeedCatalogs = "MASTER.CATALOGS.SEED";
    }

    /// <summary>Identity / admin permissions — role and permission management.</summary>
    public static class Admin
    {
        public const string RoleRead = "ADMIN.ROLE.READ";
        public const string RoleWrite = "ADMIN.ROLE.WRITE";
        public const string RoleDelete = "ADMIN.ROLE.DELETE";
        public const string PermRead = "ADMIN.PERM.READ";
        public const string PermAssign = "ADMIN.PERM.ASSIGN";
        public const string UserRead = "ADMIN.USER.READ";
        public const string UserWrite = "ADMIN.USER.WRITE";
    }

    /// <summary>Cases module permissions.</summary>
    public static class Cases
    {
        public const string Read = "CASES.CASE.READ";
        public const string Write = "CASES.CASE.WRITE";
        public const string Delete = "CASES.CASE.DELETE";
        public const string Approve = "CASES.CASE.APPROVE";
    }

    /// <summary>Files module permissions.</summary>
    public static class Files
    {
        public const string Read = "FILES.FILE.READ";
        public const string Upload = "FILES.FILE.UPLOAD";
        public const string Delete = "FILES.FILE.DELETE";
        public const string Download = "FILES.FILE.DOWNLOAD";
    }

    /// <summary>Audit module permissions.</summary>
    public static class Audit
    {
        public const string Read = "AUDIT.LOG.READ";
        public const string Export = "AUDIT.LOG.EXPORT";
        public const string Rtbf = "AUDIT.RTBF.EXECUTE";
    }

    /// <summary>Notification module permissions.</summary>
    public static class Notifications
    {
        public const string Read = "NOTIF.MSG.READ";
        public const string Send = "NOTIF.MSG.SEND";
        public const string TemplateManage = "NOTIF.TEMPLATE.MANAGE";
    }
}
