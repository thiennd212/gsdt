
namespace GSDT.MasterData.Application.Commands.UpdateDictionary;

public sealed record UpdateDictionaryCommand(
    Guid Id,
    string Name,
    string NameVi,
    string? Description,
    Guid ActorId) : ICommand;
