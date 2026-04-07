
namespace GSDT.Identity.Application.Queries.ListJitProviderConfigs;

public sealed record ListJitProviderConfigsQuery(
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<JitProviderConfigDto>>;
