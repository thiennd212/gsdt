using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.CreateCompliancePolicy;

public sealed class CreateCompliancePolicyCommandHandler(ICompliancePolicyRepository repository)
    : IRequestHandler<CreateCompliancePolicyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateCompliancePolicyCommand request,
        CancellationToken cancellationToken)
    {
        var policy = CompliancePolicy.Create(
            request.Name,
            request.Category,
            request.Rules,
            request.Enforcement);

        await repository.AddAsync(policy, cancellationToken);
        return Result.Ok(policy.Id);
    }
}
