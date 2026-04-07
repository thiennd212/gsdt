
namespace GSDT.MasterData.Entities;

/// <summary>
/// A single lookup value within a Dictionary.
/// Supports self-referencing tree (ParentId) for hierarchical dictionaries.
/// </summary>
public class DictionaryItem : AuditableEntity<Guid>, ITenantScoped
{
    public Guid DictionaryId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string NameVi { get; private set; } = string.Empty;

    /// <summary>Optional parent for tree structures (null = root item).</summary>
    public Guid? ParentId { get; private set; }

    public int SortOrder { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>Arbitrary provider-specific metadata stored as JSON.</summary>
    public string? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    // Navigation
    public Dictionary Dictionary { get; private set; } = null!;
    public DictionaryItem? Parent { get; private set; }
    public ICollection<DictionaryItem> Children { get; private set; } = new List<DictionaryItem>();

    private DictionaryItem() { }

    public static DictionaryItem Create(
        Guid dictionaryId, string code, string name, string nameVi,
        Guid? parentId, int sortOrder, DateTime effectiveFrom,
        DateTime? effectiveTo, string? metadata, Guid tenantId, Guid createdBy)
    {
        var item = new DictionaryItem
        {
            Id = Guid.NewGuid(),
            DictionaryId = dictionaryId,
            Code = code,
            Name = name,
            NameVi = nameVi,
            ParentId = parentId,
            SortOrder = sortOrder,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true,
            Metadata = metadata,
            TenantId = tenantId
        };
        item.SetAuditCreate(createdBy);
        return item;
    }

    public void Update(
        string name, string nameVi, Guid? parentId,
        int sortOrder, DateTime? effectiveTo,
        string? metadata, Guid modifiedBy)
    {
        Name = name;
        NameVi = nameVi;
        ParentId = parentId;
        SortOrder = sortOrder;
        EffectiveTo = effectiveTo;
        Metadata = metadata;
        SetAuditUpdate(modifiedBy);
    }

    public void Deactivate(Guid modifiedBy)
    {
        IsActive = false;
        SetAuditUpdate(modifiedBy);
    }
}
