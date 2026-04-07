using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetRtbfRequests;

public sealed record GetRtbfRequestsQuery(
    Guid? TenantId,
    RtbfStatus? Status,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<RtbfRequestDto>>>;
