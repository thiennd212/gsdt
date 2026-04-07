
namespace GSDT.Integration.Infrastructure.Persistence.Configurations;

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.DataScopeJson).HasColumnType("nvarchar(max)");

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(30).IsRequired();

        builder.HasIndex(c => new { c.TenantId, c.PartnerId });
        builder.HasIndex(c => new { c.TenantId, c.Status });

        // FK to Partner — restrict delete (cannot delete partner with contracts)
        builder.HasOne<Partner>()
            .WithMany()
            .HasForeignKey(c => c.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
