
namespace GSDT.Audit.Domain.Repositories;

/// <summary>Repository for CompliancePolicy and CompliancePolicyEvaluation.</summary>
public interface ICompliancePolicyRepository
{
    Task AddAsync(CompliancePolicy policy, CancellationToken cancellationToken = default);
    Task<CompliancePolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task AddEvaluationAsync(CompliancePolicyEvaluation evaluation, CancellationToken cancellationToken = default);
}
