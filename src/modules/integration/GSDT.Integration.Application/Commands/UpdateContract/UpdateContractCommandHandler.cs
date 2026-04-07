using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Commands.UpdateContract;

public sealed class UpdateContractCommandHandler(
    IContractRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateContractCommand, Result<ContractDto>>
{
    public async Task<Result<ContractDto>> Handle(
        UpdateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (contract is null)
            return Result.Fail(new NotFoundError($"Contract {request.Id} not found."));

        contract.Update(request.Title, request.Description,
            request.EffectiveDate, request.ExpiryDate,
            request.DataScopeJson, currentUser.UserId);

        await repository.UpdateAsync(contract, cancellationToken);

        return Result.Ok(new ContractDto(
            contract.Id, contract.TenantId, contract.PartnerId,
            contract.Title, contract.Description,
            contract.EffectiveDate, contract.ExpiryDate,
            contract.Status.ToString(), contract.DataScopeJson,
            contract.CreatedAt, contract.UpdatedAt));
    }
}
