using FluentResults;

namespace GSDT.Identity.Application.Queries.ListActiveSessions;

/// <summary>Returns active token metadata for session management admin view, paginated.</summary>
public sealed record ListActiveSessionsQuery(
    Guid? UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<ActiveSessionDto>>;
