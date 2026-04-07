
namespace GSDT.Integration.Application.Queries.ListPartners;

public sealed record ListPartnersQuery(
    Guid TenantId, string? SearchTerm = null,
    int Page = 1, int PageSize = 20) : IQuery<PagedResult<PartnerDto>>;
