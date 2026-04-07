
namespace GSDT.Notifications.Infrastructure.Persistence;

public sealed class NotificationTemplateEntityConfiguration : EntityTypeConfigurationBase<NotificationTemplate, Guid>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.Property(t => t.TemplateKey).HasMaxLength(100).IsRequired();
        builder.Property(t => t.SubjectTemplate).HasMaxLength(500).IsRequired();
        builder.Property(t => t.BodyTemplate).HasMaxLength(20_000).IsRequired();
        builder.Property(t => t.Channel)
            .HasConversion(c => c.Value, v => Domain.ValueObjects.NotificationChannel.From(v))
            .HasMaxLength(10).IsRequired();

        // Unique per tenant + channel + key (prevents duplicate template keys)
        builder.HasIndex(t => new { t.TenantId, t.TemplateKey, t.Channel })
            .HasDatabaseName("IX_NotificationTemplate_TenantId_Key_Channel")
            .IsUnique();
    }
}
