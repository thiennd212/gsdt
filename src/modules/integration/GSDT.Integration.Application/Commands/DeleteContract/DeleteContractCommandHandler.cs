using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Commands.DeleteContract;

public sealed class DeleteContractCommandHandler(IContractRepository repository)
    : IRequestHandler<DeleteContractCommand, Result>
{
    public async Task<Result> Handle(
        DeleteContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (contract is null)
            return Result.Fail(new NotFoundError($"Contract {request.Id} not found."));

        contract.Delete();
        await repository.UpdateAsync(contract, cancellationToken);
        return Result.Ok();
    }
}
