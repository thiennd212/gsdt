using GSDT.SharedKernel.Domain;

namespace GSDT.Concurrency.Tests.Fixtures;

/// <summary>
/// Test-only ITenantContext — identical to the one in TenantIsolation.Tests.
/// Injects a fixed TenantId and IsSystemAdmin flag into CasesDbContext.
/// </summary>
internal sealed class TestTenantContext(Guid? tenantId, bool isSystemAdmin = false) : ITenantContext
{
    public Guid? TenantId { get; } = tenantId;
    public bool IsSystemAdmin { get; } = isSystemAdmin;
}
