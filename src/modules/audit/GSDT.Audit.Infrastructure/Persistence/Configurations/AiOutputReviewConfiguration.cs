
namespace GSDT.Audit.Infrastructure.Persistence.Configurations;

public sealed class AiOutputReviewConfiguration : IEntityTypeConfiguration<AiOutputReview>
{
    public void Configure(EntityTypeBuilder<AiOutputReview> b)
    {
        b.ToTable("AiOutputReviews");
        b.HasKey(e => e.Id);

        b.Property(e => e.Decision).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(e => e.Reason).HasMaxLength(2000);

        b.HasIndex(e => e.PromptTraceId);
        b.HasIndex(e => e.Decision);
    }
}
