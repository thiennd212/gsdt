using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for BidPackage, BidItem, and Contract entities.</summary>
internal sealed class BidPackageConfiguration : IEntityTypeConfiguration<BidPackage>
{
    public void Configure(EntityTypeBuilder<BidPackage> builder)
    {
        builder.ToTable("BidPackages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ResultDecisionNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.WinningPrice).HasPrecision(18, 4);
        builder.Property(x => x.EstimatedPrice).HasPrecision(18, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasMany(x => x.BidItems)
            .WithOne(x => x.BidPackage).HasForeignKey(x => x.BidPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Contracts)
            .WithOne(x => x.BidPackage).HasForeignKey(x => x.BidPackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class BidItemConfiguration : IEntityTypeConfiguration<BidItem>
{
    public void Configure(EntityTypeBuilder<BidItem> builder)
    {
        builder.ToTable("BidItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.EstimatedPrice).HasPrecision(18, 4);
    }
}

internal sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContractNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ContractValue).HasPrecision(18, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
