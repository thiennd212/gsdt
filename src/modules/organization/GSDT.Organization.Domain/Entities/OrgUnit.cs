
namespace GSDT.Organization.Domain.Entities;

/// <summary>
/// Organization unit in Vietnamese GOV hierarchy (Ministry → Department → Division → Team).
/// Uses adjacency list (ParentId) — recursive CTE for tree queries via Dapper.
/// </summary>
public class OrgUnit : AuditableEntity<Guid>
{
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public int Level { get; private set; }
    public bool IsActive { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? SuccessorId { get; private set; }

    private readonly List<OrgUnit> _children = [];
    public IReadOnlyList<OrgUnit> Children => _children.AsReadOnly();

    private OrgUnit() { }

    public static OrgUnit Create(
        string name, string nameEn, string code,
        Guid tenantId, Guid? parentId = null, int level = 1)
    {
        return new OrgUnit
        {
            Id = Guid.NewGuid(),
            Name = name,
            NameEn = nameEn,
            Code = code,
            TenantId = tenantId,
            ParentId = parentId,
            Level = level,
            IsActive = true
        };
    }

    public void Update(string name, string nameEn)
    {
        Name = name;
        NameEn = nameEn;
        MarkUpdated();
    }

    public void SetSuccessor(Guid successorId)
    {
        SuccessorId = successorId;
        MarkUpdated();
    }

    /// <summary>Soft-deactivate. Only call after verifying no active children remain.</summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
