
namespace GSDT.Integration.Application.Queries.ListContracts;

public sealed record ListContractsQuery(
    Guid TenantId, Guid? PartnerId = null, string? SearchTerm = null,
    int Page = 1, int PageSize = 20) : IQuery<PagedResult<ContractDto>>;
