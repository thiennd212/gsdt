using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetLoginAudit;

public sealed record GetLoginAuditQuery(
    Guid? UserId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    bool? Success,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<LoginAttemptDto>>>;
