
namespace GSDT.Identity.Application.Queries.ListExternalIdentities;

public sealed record ListExternalIdentitiesQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<ExternalIdentityDto>>;
