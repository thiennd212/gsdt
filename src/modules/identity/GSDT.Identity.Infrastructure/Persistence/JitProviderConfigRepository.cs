
namespace GSDT.Identity.Infrastructure.Persistence;

/// <summary>EF Core repository for JitProviderConfig — write-side only.</summary>
public sealed class JitProviderConfigRepository(IdentityDbContext db) : IJitProviderConfigRepository
{
    public async Task<JitProviderConfig?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.JitProviderConfigs.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<JitProviderConfig?> GetBySchemeAsync(string scheme, CancellationToken ct = default)
        => await db.JitProviderConfigs.FirstOrDefaultAsync(e => e.Scheme == scheme, ct);

    public async Task AddAsync(JitProviderConfig entity, CancellationToken ct = default)
    {
        db.JitProviderConfigs.Add(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(JitProviderConfig entity, CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}
