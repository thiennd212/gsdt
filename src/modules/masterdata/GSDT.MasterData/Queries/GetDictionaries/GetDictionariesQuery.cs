
namespace GSDT.MasterData.Queries.GetDictionaries;

public sealed record GetDictionariesQuery(
    Guid TenantId,
    DictionaryStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<DictionaryDto>>;
