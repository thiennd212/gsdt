
namespace GSDT.Audit.Domain.Entities;

/// <summary>
/// Result of evaluating an entity against a CompliancePolicy — M15 AI Governance.
/// Append-only record created by EvaluateCompliancePolicyHandler.
/// </summary>
public sealed class CompliancePolicyEvaluation : AuditableEntity<Guid>
{
    public Guid PolicyId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public EvaluationResult Result { get; private set; }
    public string? Details { get; private set; }
    public DateTime EvaluatedAt { get; private set; }

    private CompliancePolicyEvaluation() { }

    public static CompliancePolicyEvaluation Create(
        Guid policyId,
        string entityType,
        Guid entityId,
        EvaluationResult result,
        string? details)
    {
        return new CompliancePolicyEvaluation
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            EntityType = entityType,
            EntityId = entityId,
            Result = result,
            Details = details,
            EvaluatedAt = DateTime.UtcNow
        };
    }
}
