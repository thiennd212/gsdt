
namespace GSDT.Integration.Application.Queries.GetMessageLog;

public sealed record GetMessageLogQuery(Guid Id, Guid TenantId) : IQuery<MessageLogDto>;
