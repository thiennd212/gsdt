
namespace GSDT.Identity.Domain.Repositories;

/// <summary>ABAC attribute rule repository — read + admin CRUD.</summary>
public interface IAttributeRuleRepository
{
    Task<IReadOnlyList<AttributeRule>> GetByAttributeAsync(string attributeKey, string attributeValue, CancellationToken ct = default);
    Task<IReadOnlyList<AttributeRule>> GetAllAsync(Guid? tenantId = null, CancellationToken ct = default);
    Task AddAsync(AttributeRule rule, CancellationToken ct = default);
    Task<AttributeRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(AttributeRule rule, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
