
namespace GSDT.MasterData.Commands.UpdateDictionary;

public sealed record UpdateDictionaryCommand(
    Guid Id,
    string Name,
    string NameVi,
    string? Description,
    Guid ActorId) : ICommand;
