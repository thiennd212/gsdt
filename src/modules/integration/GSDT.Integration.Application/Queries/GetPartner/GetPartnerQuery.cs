
namespace GSDT.Integration.Application.Queries.GetPartner;

public sealed record GetPartnerQuery(Guid Id, Guid TenantId) : IQuery<PartnerDto>;
