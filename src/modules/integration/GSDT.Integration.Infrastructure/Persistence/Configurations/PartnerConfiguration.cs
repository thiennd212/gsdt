
namespace GSDT.Integration.Infrastructure.Persistence.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ContactEmail).HasMaxLength(256);
        builder.Property(p => p.ContactPhone).HasMaxLength(30);
        builder.Property(p => p.Endpoint).HasMaxLength(500);
        builder.Property(p => p.AuthScheme).HasMaxLength(100);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(30).IsRequired();

        builder.HasIndex(p => new { p.TenantId, p.Status });
        builder.HasIndex(p => new { p.TenantId, p.Code }).IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Ignore(p => p.DomainEvents);
    }
}
