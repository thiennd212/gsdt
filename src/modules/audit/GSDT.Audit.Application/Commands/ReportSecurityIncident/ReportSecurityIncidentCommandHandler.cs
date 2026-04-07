using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.ReportSecurityIncident;

public sealed class ReportSecurityIncidentCommandHandler(ISecurityIncidentRepository repository)
    : IRequestHandler<ReportSecurityIncidentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        ReportSecurityIncidentCommand request,
        CancellationToken cancellationToken)
    {
        var incident = SecurityIncident.Report(
            request.TenantId,
            request.Title,
            request.Severity,
            request.Description,
            request.ReportedBy,
            request.OccurredAt);

        await repository.AddAsync(incident, cancellationToken);
        return Result.Ok(incident.Id);
    }
}
