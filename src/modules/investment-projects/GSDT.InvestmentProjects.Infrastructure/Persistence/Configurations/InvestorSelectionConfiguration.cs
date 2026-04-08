namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configurations for InvestorSelection and InvestorSelectionInvestor.
/// InvestorSelection: shared PK (ProjectId), FK → InvestmentProject base.
/// InvestorSelectionInvestor: composite PK (InvestorSelectionId, InvestorId) — junction table only.
/// InvestorId is a cross-module Guid reference — no FK constraint to Investor entity.
/// </summary>
internal sealed class InvestorSelectionConfiguration : IEntityTypeConfiguration<InvestorSelection>
{
    public void Configure(EntityTypeBuilder<InvestorSelection> builder)
    {
        builder.ToTable("InvestorSelections");

        // Shared PK — no separate Id column
        builder.HasKey(x => x.ProjectId);

        builder.Property(x => x.SelectionMethod).HasMaxLength(200);
        builder.Property(x => x.SelectionDecisionNumber).HasMaxLength(100);

        builder.Property(x => x.RowVersion).IsRowVersion();

        // FK → InvestmentProject base (NOT PppProject — allows DNNN reuse)
        builder.HasOne(x => x.Project)
            .WithOne(x => x.InvestorSelection)
            .HasForeignKey<InvestorSelection>(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Investors)
            .WithOne()
            .HasForeignKey(x => x.InvestorSelectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class InvestorSelectionInvestorConfiguration
    : IEntityTypeConfiguration<InvestorSelectionInvestor>
{
    public void Configure(EntityTypeBuilder<InvestorSelectionInvestor> builder)
    {
        builder.ToTable("InvestorSelectionInvestors");

        // Composite PK — no surrogate key
        builder.HasKey(x => new { x.InvestorSelectionId, x.InvestorId });

        // No FK to Investor entity (cross-module boundary — Guid reference only)
    }
}
