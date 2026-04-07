
namespace GSDT.Notifications.Infrastructure.Persistence;

public sealed class NotificationEntityConfiguration : EntityTypeConfigurationBase<Notification, Guid>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Subject).HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(20_000).IsRequired();
        builder.Property(n => n.CorrelationId).HasMaxLength(200);
        builder.Property(n => n.Channel)
            .HasConversion(c => c.Value, v => Domain.ValueObjects.NotificationChannel.From(v))
            .HasMaxLength(10).IsRequired();
        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(20).IsRequired();

        // Index for fast unread count and list queries
        builder.HasIndex(n => new { n.RecipientUserId, n.TenantId, n.IsRead })
            .HasDatabaseName("IX_Notification_Recipient_Tenant_IsRead");
    }
}
