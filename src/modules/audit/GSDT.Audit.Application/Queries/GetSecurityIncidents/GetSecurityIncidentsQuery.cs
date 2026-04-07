using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetSecurityIncidents;

public sealed record GetSecurityIncidentsQuery(
    Guid? TenantId,
    AuditSeverity? Severity,
    IncidentStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<SecurityIncidentDto>>>;
