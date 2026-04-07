using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.CreateCompliancePolicy;

public sealed record CreateCompliancePolicyCommand(
    string Name,
    ComplianceCategory Category,
    string Rules,
    PolicyEnforcement Enforcement) : IRequest<Result<Guid>>;
