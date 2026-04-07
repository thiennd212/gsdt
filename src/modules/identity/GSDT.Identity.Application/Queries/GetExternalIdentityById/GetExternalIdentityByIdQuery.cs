
namespace GSDT.Identity.Application.Queries.GetExternalIdentityById;

public sealed record GetExternalIdentityByIdQuery(Guid Id) : IQuery<ExternalIdentityDto>;
