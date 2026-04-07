using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetAiPromptTraces;

public sealed record GetAiPromptTracesQuery(
    Guid TenantId,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<AiPromptTraceDto>>>;
