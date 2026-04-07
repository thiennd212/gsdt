
namespace GSDT.Files.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF config for RetentionPolicy.
/// DocumentType indexed with TenantId for policy lookup during file classification.
/// Only active policies fetched by enforcement job — IsActive index included.
/// </summary>
public sealed class RetentionPolicyConfiguration
    : EntityTypeConfigurationBase<RetentionPolicy, Guid>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RetentionPolicy> builder)
    {
        builder.ToTable("RetentionPolicies");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.DocumentType).HasMaxLength(100).IsRequired();
        builder.Property(p => p.RetainDays).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();

        // Policy lookup by document type within tenant
        builder.HasIndex(p => new { p.TenantId, p.DocumentType, p.IsActive })
            .HasDatabaseName("IX_RetentionPolicies_TenantId_DocumentType_IsActive");
    }
}
