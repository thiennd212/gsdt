
namespace GSDT.MasterData.Queries.GetDictionaryItems;

public sealed record GetDictionaryItemsQuery(
    Guid DictionaryId,
    bool ActiveOnly = true,
    int Page = 1,
    int PageSize = 100) : IQuery<PagedResult<DictionaryItemDto>>;
