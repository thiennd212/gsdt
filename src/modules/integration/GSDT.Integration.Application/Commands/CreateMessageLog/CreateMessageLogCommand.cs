
namespace GSDT.Integration.Application.Commands.CreateMessageLog;

public sealed record CreateMessageLogCommand(
    Guid TenantId, Guid PartnerId, Guid? ContractId,
    MessageDirection Direction, string MessageType,
    string? Payload, string? CorrelationId) : ICommand<MessageLogDto>;
