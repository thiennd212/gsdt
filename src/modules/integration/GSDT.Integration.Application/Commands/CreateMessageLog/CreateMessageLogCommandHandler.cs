using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Commands.CreateMessageLog;

public sealed class CreateMessageLogCommandHandler(
    IMessageLogRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<CreateMessageLogCommand, Result<MessageLogDto>>
{
    public async Task<Result<MessageLogDto>> Handle(
        CreateMessageLogCommand request, CancellationToken cancellationToken)
    {
        var log = MessageLog.Create(
            request.TenantId, request.PartnerId, request.ContractId,
            request.Direction, request.MessageType, request.Payload,
            request.CorrelationId, currentUser.UserId);

        await repository.AddAsync(log, cancellationToken);

        return Result.Ok(MapToDto(log));
    }

    private static MessageLogDto MapToDto(MessageLog m) => new(
        m.Id, m.TenantId, m.PartnerId, m.ContractId,
        m.Direction.ToString(), m.MessageType, m.Payload,
        m.Status.ToString(), m.CorrelationId,
        m.SentAt, m.AcknowledgedAt, m.CreatedAt, m.UpdatedAt);
}
