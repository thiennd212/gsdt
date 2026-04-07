using GSDT.ModuleName.Domain.Entities;
using GSDT.ModuleName.Domain.Repositories;
using FluentResults;
using MediatR;

namespace GSDT.ModuleName.Application.Commands.CreateModuleName;

public sealed class CreateModuleNameCommandHandler(IModuleNameRepository repository)
    : IRequestHandler<CreateModuleNameCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateModuleNameCommand request,
        CancellationToken cancellationToken)
    {
        var entity = ModuleNameEntity.Create(
            request.TenantId,
            request.Title,
            request.Description,
            request.CreatedBy);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Ok(entity.Id);
    }
}
