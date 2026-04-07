
namespace GSDT.MasterData.Application.Commands.CreateDictionary;

public sealed record CreateDictionaryCommand(
    string Code,
    string Name,
    string NameVi,
    string? Description,
    Guid TenantId,
    bool IsSystemDefined,
    Guid ActorId) : ICommand<Guid>;
