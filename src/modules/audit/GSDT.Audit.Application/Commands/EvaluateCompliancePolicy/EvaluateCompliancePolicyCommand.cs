using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.EvaluateCompliancePolicy;

public sealed record EvaluateCompliancePolicyCommand(
    Guid PolicyId,
    string EntityType,
    Guid EntityId) : IRequest<Result<CompliancePolicyEvaluationDto>>;
