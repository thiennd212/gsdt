
namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

public sealed class ExternalIdentityConfiguration : IEntityTypeConfiguration<ExternalIdentity>
{
    public void Configure(EntityTypeBuilder<ExternalIdentity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExternalId).HasMaxLength(500).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(200);
        builder.Property(e => e.Email).HasMaxLength(254);
        builder.Property(e => e.Metadata).HasColumnType("nvarchar(max)");
        builder.Property(e => e.Provider).IsRequired();

        // One user can have at most one external identity per provider
        builder.HasIndex(e => new { e.UserId, e.Provider }).IsUnique();
        // External ID must be unique within a provider
        builder.HasIndex(e => new { e.Provider, e.ExternalId }).IsUnique();
        builder.HasIndex(e => e.UserId);

        // Relationship: ExternalIdentity → ApplicationUser (many-to-one)
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
