
namespace GSDT.Notifications.Infrastructure.Persistence;

public sealed class NotificationLogEntityConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.CorrelationId).HasMaxLength(200).IsRequired();

        // Unique index enforces dedup guarantee (S52 spec)
        builder.HasIndex(l => new { l.TemplateId, l.RecipientId, l.CorrelationId })
            .HasDatabaseName("IX_NotificationLog_TemplateId_RecipientId_CorrelationId")
            .IsUnique();
    }
}
