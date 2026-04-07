using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.GetExternalIdentityById;

public sealed class GetExternalIdentityByIdQueryHandler(
    IExternalIdentityRepository repository)
    : IRequestHandler<GetExternalIdentityByIdQuery, Result<ExternalIdentityDto>>
{
    public async Task<Result<ExternalIdentityDto>> Handle(
        GetExternalIdentityByIdQuery query, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(query.Id, ct);
        if (entity is null)
            return Result.Fail<ExternalIdentityDto>(
                new NotFoundError($"ExternalIdentity {query.Id} not found"));

        return Result.Ok(new ExternalIdentityDto(
            entity.Id, entity.UserId, entity.Provider,
            entity.ExternalId, entity.DisplayName, entity.Email,
            entity.LinkedAt, entity.LastSyncAt, entity.IsActive));
    }
}
