
namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserDelegationConfiguration : IEntityTypeConfiguration<UserDelegation>
{
    public void Configure(EntityTypeBuilder<UserDelegation> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Reason).HasMaxLength(500);
        builder.HasIndex(d => new { d.DelegateId, d.IsRevoked, d.ValidTo });
        builder.HasIndex(d => d.DelegatorId);
    }
}

public sealed class AccessReviewRecordConfiguration : IEntityTypeConfiguration<AccessReviewRecord>
{
    public void Configure(EntityTypeBuilder<AccessReviewRecord> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.UserId, r.Decision });
        builder.HasIndex(r => r.NextReviewDue);
    }
}

public sealed class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Purpose).HasMaxLength(200).IsRequired();
        builder.Property(c => c.LegalBasis).HasMaxLength(100).IsRequired();
        builder.Property(c => c.DataSubjectType).HasMaxLength(50).IsRequired(false);
        builder.Property(c => c.EvidenceJson).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.HasIndex(c => new { c.DataSubjectId, c.IsWithdrawn });
        builder.HasIndex(c => c.TenantId);
    }
}

public sealed class AttributeRuleConfiguration : IEntityTypeConfiguration<AttributeRule>
{
    public void Configure(EntityTypeBuilder<AttributeRule> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Resource).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Action).HasMaxLength(100).IsRequired();
        builder.Property(r => r.AttributeKey).HasMaxLength(100).IsRequired();
        builder.Property(r => r.AttributeValue).HasMaxLength(200).IsRequired();
        builder.HasIndex(r => new { r.AttributeKey, r.AttributeValue, r.TenantId });
    }
}
