using System.Text.Json;

namespace GSDT.Identity.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.DepartmentCode).HasMaxLength(50);
        builder.Property(u => u.ClearanceLevel).IsRequired();

        // ExtraProperties: JSON column — teams extend without forking
        // ValueComparer ensures EF Core correctly detects dictionary mutations during change tracking
        var extraPropsComparer = new ValueComparer<Dictionary<string, JsonElement>>(
            (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) ==
                      JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
            v => JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                (JsonSerializerOptions?)null) ?? new Dictionary<string, JsonElement>());

        builder.Property(u => u.ExtraProperties)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(v,
                    (JsonSerializerOptions?)null) ?? new Dictionary<string, JsonElement>())
            .Metadata.SetValueComparer(extraPropsComparer);

        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => u.DepartmentCode);
        builder.HasIndex(u => new { u.TenantId, u.IsActive });

        // Delegations navigation
        builder.HasMany(u => u.DelegationsAsDelegate)
            .WithOne(d => d.Delegate)
            .HasForeignKey(d => d.DelegateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.DelegationsAsDelegator)
            .WithOne(d => d.Delegator)
            .HasForeignKey(d => d.DelegatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
