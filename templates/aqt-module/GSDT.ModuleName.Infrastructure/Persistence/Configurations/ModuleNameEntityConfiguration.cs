using GSDT.ModuleName.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GSDT.ModuleName.Infrastructure.Persistence.Configurations;

public sealed class ModuleNameEntityConfiguration : IEntityTypeConfiguration<ModuleNameEntity>
{
    public void Configure(EntityTypeBuilder<ModuleNameEntity> builder)
    {
        builder.ToTable("ModuleNames");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.HasIndex(e => new { e.TenantId, e.Title });
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
