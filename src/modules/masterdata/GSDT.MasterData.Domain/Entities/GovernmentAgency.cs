
namespace GSDT.MasterData.Domain.Entities;

/// <summary>
/// Cơ quan chủ quản (GovernmentAgency) — tenant-scoped, hierarchical (self-reference).
/// Covers "Các Tỉnh", "Các Bộ, Ban ngành", etc. (SRS T27).
/// </summary>
public class GovernmentAgency : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public Guid? ParentId { get; set; }
    public string? AgencyType { get; set; }      // "Các Tỉnh", "Các Bộ, Ban ngành", etc.
    public string? Origin { get; set; }
    public string? LdaServer { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; } = 0;
    public int? ReportDisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // ── Navigation ────────────────────────────────────────────────────────────
    public GovernmentAgency? Parent { get; set; }
    public ICollection<GovernmentAgency> Children { get; set; } = [];

    private GovernmentAgency() { }

    public static GovernmentAgency Create(
        Guid tenantId, string name, string code, Guid? parentId = null)
    {
        var agency = new GovernmentAgency();
        agency.Id       = Guid.NewGuid();
        agency.TenantId = tenantId;
        agency.Name     = name;
        agency.Code     = code;
        agency.ParentId = parentId;
        agency.IsActive = true;
        return agency;
    }
}
