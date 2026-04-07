using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Commands.CreateContract;

public sealed class CreateContractCommandHandler(
    IContractRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<CreateContractCommand, Result<ContractDto>>
{
    public async Task<Result<ContractDto>> Handle(
        CreateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = Contract.Create(
            request.TenantId, request.PartnerId, request.Title, request.Description,
            request.EffectiveDate, request.ExpiryDate, request.DataScopeJson,
            currentUser.UserId);

        await repository.AddAsync(contract, cancellationToken);

        return Result.Ok(MapToDto(contract));
    }

    private static ContractDto MapToDto(Contract c) => new(
        c.Id, c.TenantId, c.PartnerId, c.Title, c.Description,
        c.EffectiveDate, c.ExpiryDate, c.Status.ToString(),
        c.DataScopeJson, c.CreatedAt, c.UpdatedAt);
}
