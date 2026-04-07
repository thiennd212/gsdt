namespace GSDT.SharedKernel.Domain;

/// <summary>
/// Base entity with typed ID, soft delete, and audit timestamps.
/// Implements ISoftDeletable — EF SoftDeleteInterceptor and global query filter rely on this.
/// </summary>
public abstract class Entity<TId> : ISoftDeletable
{
    public TId Id { get; protected set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    /// <summary>Marks entity as soft-deleted. Called by SoftDeleteInterceptor on EF Delete.</summary>
    public void MarkDeleted() => IsDeleted = true;

    protected void MarkUpdated() => UpdatedAt = DateTimeOffset.UtcNow;

    /// <summary>Alias for MarkDeleted — use in domain logic for intent clarity.</summary>
    public void Delete() => MarkDeleted();
}
