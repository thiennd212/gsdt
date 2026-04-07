namespace GSDT.MasterData.Entities;

/// <summary>
/// Kế hoạch lựa chọn nhà thầu (ContractorSelectionPlan / KHLCNT) — tenant-scoped, auditable.
/// OrderNumber is auto-incremented per tenant.
/// </summary>
public class ContractorSelectionPlan : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public int OrderNumber { get; set; }
    public string NameVi { get; set; } = default!;
    public string NameEn { get; set; } = default!;
    public DateTime SignedDate { get; set; }
    public string SignedBy { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    private ContractorSelectionPlan() { }

    public static ContractorSelectionPlan Create(
        Guid tenantId, string nameVi, string nameEn, DateTime signedDate, string signedBy)
    {
        // Use protected setter via subclass — assign Id inside the class context.
        var plan = new ContractorSelectionPlan();
        plan.Id         = Guid.NewGuid();
        plan.TenantId   = tenantId;
        plan.NameVi     = nameVi;
        plan.NameEn     = nameEn;
        plan.SignedDate  = signedDate;
        plan.SignedBy    = signedBy;
        plan.IsActive   = true;
        return plan;
    }
}
