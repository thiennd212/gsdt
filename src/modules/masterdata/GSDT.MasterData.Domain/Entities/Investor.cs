
namespace GSDT.MasterData.Domain.Entities;

/// <summary>
/// Chủ đầu tư (Investor) — tenant-scoped, flat catalog (SRS T28).
/// InvestorType: "Doanh nghiệp", "Cá nhân", "Tổ chức khác".
/// BusinessIdOrCccd: MST for enterprises, CCCD for individuals.
/// </summary>
public class Investor : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string InvestorType { get; set; } = default!;      // "Doanh nghiệp" | "Cá nhân" | "Tổ chức khác"
    public string BusinessIdOrCccd { get; set; } = default!;  // MST or CCCD
    public string NameVi { get; set; } = default!;
    public string? NameEn { get; set; }
    public bool IsActive { get; set; } = true;

    private Investor() { }

    public static Investor Create(
        Guid tenantId, string investorType, string businessIdOrCccd,
        string nameVi, string? nameEn = null)
    {
        var investor = new Investor();
        investor.Id                = Guid.NewGuid();
        investor.TenantId          = tenantId;
        investor.InvestorType      = investorType;
        investor.BusinessIdOrCccd  = businessIdOrCccd;
        investor.NameVi            = nameVi;
        investor.NameEn            = nameEn;
        investor.IsActive          = true;
        return investor;
    }
}
