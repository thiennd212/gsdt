namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for RegistrationCertificate (GCNĐKĐT) — investment.RegistrationCertificates.
/// FK to InvestmentProject base so DNNN/NĐT/FDI can all use it.
/// </summary>
internal sealed class RegistrationCertificateConfiguration : IEntityTypeConfiguration<RegistrationCertificate>
{
    public void Configure(EntityTypeBuilder<RegistrationCertificate> builder)
    {
        builder.ToTable("RegistrationCertificates");

        builder.Property(x => x.CertificateNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Capital — precision (18,4)
        builder.Property(x => x.InvestmentCapital).HasPrecision(18, 4);
        builder.Property(x => x.EquityCapital).HasPrecision(18, 4);
        builder.Property(x => x.EquityRatio).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.ProjectId);

        // FK to base InvestmentProject — reusable by NĐT and FDI
        builder.HasOne(x => x.Project)
            .WithMany(x => x.RegistrationCertificates)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
