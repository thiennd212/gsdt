using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.UpdateIncidentStatus;

public sealed record UpdateIncidentStatusCommand(
    Guid IncidentId,
    IncidentStatus NewStatus,
    string? MitigationNote) : IRequest<Result>;
