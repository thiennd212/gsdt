
namespace GSDT.Audit.Infrastructure.Persistence.Configurations;

public sealed class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> b)
    {
        b.ToTable("LoginAttempts");
        b.HasKey(e => e.Id);

        b.Property(e => e.Email).HasMaxLength(256).IsRequired();
        b.Property(e => e.IpAddress).HasMaxLength(64).IsRequired();
        b.Property(e => e.UserAgent).HasMaxLength(512);
        b.Property(e => e.FailureReason).HasMaxLength(256);

        b.HasIndex(e => new { e.UserId, e.AttemptedAt });
        b.HasIndex(e => new { e.IpAddress, e.AttemptedAt });
        b.HasIndex(e => e.AttemptedAt);
    }
}
