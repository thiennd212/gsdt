
namespace GSDT.Integration.Application.Commands.UpdateMessageLogStatus;

public sealed record UpdateMessageLogStatusCommand(
    Guid Id, MessageLogStatus NewStatus) : ICommand<MessageLogDto>;
