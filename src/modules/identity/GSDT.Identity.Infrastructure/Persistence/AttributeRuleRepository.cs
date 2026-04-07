
namespace GSDT.Identity.Infrastructure.Persistence;

public sealed class AttributeRuleRepository : IAttributeRuleRepository
{
    private readonly IdentityDbContext _db;

    public AttributeRuleRepository(IdentityDbContext db) => _db = db;

    public async Task<IReadOnlyList<AttributeRule>> GetByAttributeAsync(
        string attributeKey, string attributeValue,
        CancellationToken ct = default)
        => await _db.AttributeRules
            .AsNoTracking()
            .Where(r => r.AttributeKey == attributeKey && r.AttributeValue == attributeValue)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AttributeRule>> GetAllAsync(Guid? tenantId = null, CancellationToken ct = default)
        => await _db.AttributeRules
            .AsNoTracking()
            .Where(r => tenantId == null || r.TenantId == tenantId)
            .OrderBy(r => r.Resource).ThenBy(r => r.Action)
            .ToListAsync(ct);

    public async Task AddAsync(AttributeRule rule, CancellationToken ct = default)
    {
        await _db.AttributeRules.AddAsync(rule, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AttributeRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.AttributeRules.FindAsync([id], ct);

    public async Task UpdateAsync(AttributeRule rule, CancellationToken ct = default)
    {
        _db.AttributeRules.Update(rule);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await _db.AttributeRules.FindAsync([id], ct);
        if (rule is not null)
        {
            _db.AttributeRules.Remove(rule);
            await _db.SaveChangesAsync(ct);
        }
    }
}
