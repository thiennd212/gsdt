
namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

public sealed class CredentialPolicyConfiguration : IEntityTypeConfiguration<CredentialPolicy>
{
    public void Configure(EntityTypeBuilder<CredentialPolicy> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();

        // Only one default policy allowed per tenant (partial unique index)
        builder.HasIndex(p => new { p.TenantId, p.IsDefault });
        builder.HasIndex(p => p.TenantId);
    }
}
