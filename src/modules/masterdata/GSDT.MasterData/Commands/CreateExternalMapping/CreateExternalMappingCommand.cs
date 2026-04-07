
namespace GSDT.MasterData.Commands.CreateExternalMapping;

public sealed record CreateExternalMappingCommand(
    string InternalCode,
    string ExternalSystem,
    string ExternalCode,
    MappingDirection Direction,
    Guid? DictionaryId,
    DateTime ValidFrom,
    DateTime? ValidTo,
    Guid TenantId,
    Guid ActorId) : ICommand<Guid>;
