namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configurations for DesignEstimate and DesignEstimateItem.
/// DesignEstimate FK → InvestmentProject base (available to both PPP and DNNN).
/// DesignEstimateItem FK → DesignEstimate with cascade delete.
/// All monetary fields precision (18,4).
/// </summary>
internal sealed class DesignEstimateConfiguration : IEntityTypeConfiguration<DesignEstimate>
{
    public void Configure(EntityTypeBuilder<DesignEstimate> builder)
    {
        builder.ToTable("DesignEstimates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApprovalDecisionNumber).HasMaxLength(100);
        builder.Property(x => x.ApprovalAuthority).HasMaxLength(200);
        builder.Property(x => x.ApprovalSigner).HasMaxLength(200);
        builder.Property(x => x.ApprovalSummary).HasMaxLength(2000);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.EquipmentCost).HasPrecision(18, 4);
        builder.Property(x => x.ConstructionCost).HasPrecision(18, 4);
        builder.Property(x => x.LandCompensationCost).HasPrecision(18, 4);
        builder.Property(x => x.ManagementCost).HasPrecision(18, 4);
        builder.Property(x => x.ConsultancyCost).HasPrecision(18, 4);
        builder.Property(x => x.ContingencyCost).HasPrecision(18, 4);
        builder.Property(x => x.OtherCost).HasPrecision(18, 4);
        builder.Property(x => x.TotalEstimate).HasPrecision(18, 4);

        builder.Property(x => x.RowVersion).IsRowVersion();

        // FK → InvestmentProject base (NOT PppProject — cross-type sharing)
        builder.HasOne(x => x.Project)
            .WithMany(x => x.DesignEstimates).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.DesignEstimate).HasForeignKey(x => x.DesignEstimateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class DesignEstimateItemConfiguration : IEntityTypeConfiguration<DesignEstimateItem>
{
    public void Configure(EntityTypeBuilder<DesignEstimateItem> builder)
    {
        builder.ToTable("DesignEstimateItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Scale).HasMaxLength(500);
        builder.Property(x => x.Cost).HasPrecision(18, 4);
    }
}
