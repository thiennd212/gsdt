
namespace GSDT.Integration.Application.Queries.ListMessageLogs;

public sealed record ListMessageLogsQuery(
    Guid TenantId, Guid? PartnerId = null, Guid? ContractId = null,
    string? SearchTerm = null, int Page = 1, int PageSize = 20) : IQuery<PagedResult<MessageLogDto>>;
