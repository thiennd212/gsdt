namespace GSDT.SharedKernel.Domain;

/// <summary>Extends Entity with user audit trail and data classification (QĐ742).</summary>
public abstract class AuditableEntity<TId> : Entity<TId>
{
    public Guid CreatedBy { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    /// <summary>Data classification per QĐ742: Public | Internal | Confidential | Secret | TopSecret.</summary>
    public ClassificationLevel ClassificationLevel { get; protected set; } = ClassificationLevel.Internal;

    public void SetAuditCreate(Guid userId) => CreatedBy = userId;

    public void SetAuditUpdate(Guid userId)
    {
        ModifiedBy = userId;
        MarkUpdated();
    }
}
