
namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

public sealed class JitProviderConfigConfiguration : IEntityTypeConfiguration<JitProviderConfig>
{
    public void Configure(EntityTypeBuilder<JitProviderConfig> builder)
    {
        builder.ToTable("JitProviderConfigs", "identity");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Scheme).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.DefaultRoleName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ClaimMappingJson).HasColumnType("nvarchar(max)");
        builder.Property(e => e.AllowedDomainsJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(e => e.Scheme).IsUnique().HasDatabaseName("IX_JitProviderConfig_Scheme");
    }
}
