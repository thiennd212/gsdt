using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.ListActiveSessions;

public sealed class ListActiveSessionsQueryHandler
    : IRequestHandler<ListActiveSessionsQuery, Result<PagedResult<ActiveSessionDto>>>
{
    private readonly ISessionRepository _repo;

    public ListActiveSessionsQueryHandler(ISessionRepository repo) => _repo = repo;

    public async Task<Result<PagedResult<ActiveSessionDto>>> Handle(
        ListActiveSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var (tokens, totalCount) = await _repo.ListActiveAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        var dtos = tokens
            .Select(t => new ActiveSessionDto(
                t.TokenId, t.UserId, t.UserEmail,
                t.IssuedAt, t.ExpiresAt, t.IpAddress, t.ClientId))
            .ToList();

        var totalPages = request.PageSize > 0
            ? (int)Math.Ceiling((double)totalCount / request.PageSize)
            : 0;
        var meta = new PaginationMeta(
            request.Page, request.PageSize, totalPages,
            null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<ActiveSessionDto>(dtos, totalCount, meta));
    }
}
