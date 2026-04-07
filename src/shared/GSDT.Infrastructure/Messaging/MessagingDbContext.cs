
namespace GSDT.Infrastructure.Messaging;

/// <summary>
/// Dedicated DbContext for cross-cutting messaging concerns:
/// dead letters (messaging.dead_letters).
/// Separate from ModuleDbContext — no tenant filter needed here.
/// Registered as Scoped by AddMessageBus().
/// </summary>
public sealed class MessagingDbContext(DbContextOptions<MessagingDbContext> options)
    : DbContext(options)
{
    public DbSet<MessageDeadLetter> DeadLetters { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("messaging");

        modelBuilder.Entity<MessageDeadLetter>(e =>
        {
            e.ToTable("dead_letters");
            e.HasKey(x => x.Id);
            e.Property(x => x.MessageType).HasMaxLength(500).IsRequired();
            e.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.FailureReason).HasMaxLength(2000).IsRequired();
            e.Property(x => x.OriginalQueue).HasMaxLength(200).IsRequired();
            e.Property(x => x.QuarantineReason).HasMaxLength(1000);
            e.Property(x => x.Status).HasConversion<int>();
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.ReceivedAt);
        });
    }
}
