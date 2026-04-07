using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.ProcessRtbfRequest;

/// <summary>Mark an RTBF request as processed (PII anonymization completed).</summary>
public sealed record ProcessRtbfRequestCommand(Guid RequestId, Guid ProcessedBy) : IRequest<Result>;
