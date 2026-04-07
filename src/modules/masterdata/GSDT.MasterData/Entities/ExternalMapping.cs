
namespace GSDT.MasterData.Entities;

/// <summary>Directionality of the code mapping between internal and external systems.</summary>
public enum MappingDirection
{
    In = 1,
    Out = 2,
    Both = 3
}

/// <summary>
/// Maps internal dictionary codes to codes used by external systems (e.g., VBQPPL, eMIS, VNPT).
/// Supports time-bound validity and bidirectional/unidirectional translation.
/// </summary>
public class ExternalMapping : AuditableEntity<Guid>, ITenantScoped
{
    public string InternalCode { get; private set; } = string.Empty;
    public string ExternalSystem { get; private set; } = string.Empty;
    public string ExternalCode { get; private set; } = string.Empty;
    public MappingDirection Direction { get; private set; }

    /// <summary>Optional link to the source dictionary (null = ad-hoc mapping without dictionary).</summary>
    public Guid? DictionaryId { get; private set; }

    public bool IsActive { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public Guid TenantId { get; private set; }

    // Navigation
    public Dictionary? Dictionary { get; private set; }

    private ExternalMapping() { }

    public static ExternalMapping Create(
        string internalCode, string externalSystem, string externalCode,
        MappingDirection direction, Guid? dictionaryId,
        DateTime validFrom, DateTime? validTo,
        Guid tenantId, Guid createdBy)
    {
        var mapping = new ExternalMapping
        {
            Id = Guid.NewGuid(),
            InternalCode = internalCode,
            ExternalSystem = externalSystem,
            ExternalCode = externalCode,
            Direction = direction,
            DictionaryId = dictionaryId,
            IsActive = true,
            ValidFrom = validFrom,
            ValidTo = validTo,
            TenantId = tenantId
        };
        mapping.SetAuditCreate(createdBy);
        return mapping;
    }

    public void Deactivate(Guid modifiedBy)
    {
        IsActive = false;
        SetAuditUpdate(modifiedBy);
    }
}
