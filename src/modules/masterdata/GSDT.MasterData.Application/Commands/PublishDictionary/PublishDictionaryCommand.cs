
namespace GSDT.MasterData.Application.Commands.PublishDictionary;

public sealed record PublishDictionaryCommand(
    Guid Id,
    string? Notes,
    Guid ActorId) : ICommand<int>;
