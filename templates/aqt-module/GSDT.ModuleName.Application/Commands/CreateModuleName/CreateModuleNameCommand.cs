using GSDT.SharedKernel.Application;

namespace GSDT.ModuleName.Application.Commands.CreateModuleName;

public sealed record CreateModuleNameCommand(
    Guid TenantId,
    string Title,
    string? Description,
    Guid CreatedBy) : ICommand<Guid>;
