using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Commands.ReviewAiOutput;

public sealed record ReviewAiOutputCommand(
    Guid PromptTraceId,
    Guid ReviewerId,
    ReviewDecision Decision,
    string? Reason) : IRequest<Result>;
