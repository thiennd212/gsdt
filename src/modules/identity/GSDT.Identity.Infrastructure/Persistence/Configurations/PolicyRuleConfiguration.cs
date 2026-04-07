
namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PolicyRule.
/// Table: identity.PolicyRules
/// Unique: Code (global), composite index on (PermissionCode, TenantId, Priority) for rule lookup.
/// </summary>
public sealed class PolicyRuleConfiguration : IEntityTypeConfiguration<PolicyRule>
{
    public void Configure(EntityTypeBuilder<PolicyRule> builder)
    {
        builder.ToTable("PolicyRules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.PermissionCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.ConditionExpression)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.Effect)
            .IsRequired();

        builder.Property(r => r.Priority)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        // Code must be globally unique
        builder.HasIndex(r => r.Code).IsUnique();

        // Composite index for efficient rule loading per permission + tenant, ordered by priority
        builder.HasIndex(r => new { r.PermissionCode, r.TenantId, r.Priority });
    }
}
