namespace GSDT.MasterData.Entities.Catalogs;

/// <summary>
/// Base class for tenant-scoped catalog entities — managed per-tenant (CRUD via CatalogsController).
/// Extends AuditableEntity for audit trail; implements ITenantScoped for EF tenant filtering.
/// </summary>
public abstract class TenantCatalog : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Initialises the base fields for a new tenant catalog item.
    /// Called by concrete subclass factory methods.
    /// Uses protected setter on Id (inherited from Entity{TId}).
    /// </summary>
    protected void InitNew(Guid tenantId, string code, string name)
    {
        Id       = Guid.NewGuid();
        TenantId = tenantId;
        Code     = code;
        Name     = name;
        IsActive = true;
    }

    /// <summary>
    /// Generic factory that creates any concrete TenantCatalog subclass via InitNew.
    /// Avoids duplicating factory methods across 10 entity classes.
    /// </summary>
    public static T Create<T>(Guid tenantId, string code, string name)
        where T : TenantCatalog, new()
    {
        var entity = new T();
        entity.InitNew(tenantId, code, name);
        return entity;
    }
}
