
namespace GSDT.Audit.Application.DTOs;

/// <summary>Read model for CompliancePolicyEvaluation.</summary>
public sealed record CompliancePolicyEvaluationDto(
    Guid Id,
    Guid PolicyId,
    string EntityType,
    Guid EntityId,
    EvaluationResult Result,
    string? Details,
    DateTime EvaluatedAt);
