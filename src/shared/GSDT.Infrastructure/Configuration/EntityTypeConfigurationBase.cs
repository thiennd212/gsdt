
namespace GSDT.Infrastructure.Configuration;

/// <summary>
/// Base EF entity configuration for all AuditableEntity subclasses.
/// Auto-applies composite indexes per perf spec (S260307):
///   - (TenantId, Id)          — primary tenant-scoped access pattern
///   - (TenantId, CreatedAt)   — list queries ordered by creation time
///   - (IsDeleted)             — partial filter for soft-delete global query filter
///
/// Usage:
///   public class NotificationConfiguration : EntityTypeConfigurationBase&lt;Notification, Guid&gt;
///   {
///       protected override void ConfigureEntity(EntityTypeBuilder&lt;Notification&gt; builder) { ... }
///   }
/// </summary>
public abstract class EntityTypeConfigurationBase<T, TId> : IEntityTypeConfiguration<T>
    where T : AuditableEntity<TId>
    where TId : notnull
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        // Soft-delete index — helps EF query filter skip deleted rows efficiently
        builder.HasIndex(nameof(Entity<TId>.IsDeleted))
            .HasDatabaseName($"IX_{typeof(T).Name}_IsDeleted");

        // Tenant-scoped composite indexes for ITenantScoped entities
        if (typeof(ITenantScoped).IsAssignableFrom(typeof(T)))
        {
            builder.HasIndex("TenantId", nameof(AuditableEntity<TId>.Id))
                .HasDatabaseName($"IX_{typeof(T).Name}_TenantId_Id");

            builder.HasIndex("TenantId", nameof(Entity<TId>.CreatedAt))
                .HasDatabaseName($"IX_{typeof(T).Name}_TenantId_CreatedAt")
                .IsDescending(false, true); // TenantId ASC, CreatedAt DESC
        }

        ConfigureEntity(builder);
    }

    /// <summary>Implement entity-specific column mappings, constraints, and indexes.</summary>
    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);

    /// <summary>
    /// Helper: adds (TenantId, {statusProperty}) composite index for workflow-state entities.
    /// Call from ConfigureEntity when the entity has a Status/State property.
    /// </summary>
    protected static void AddStatusIndex(EntityTypeBuilder<T> builder, string statusPropertyName = "Status")
    {
        builder.HasIndex("TenantId", statusPropertyName)
            .HasDatabaseName($"IX_{typeof(T).Name}_TenantId_{statusPropertyName}");
    }
}
