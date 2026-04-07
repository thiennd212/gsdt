using GSDT.ModuleName.Application.DTOs;
using GSDT.SharedKernel.Application;

namespace GSDT.ModuleName.Application.Queries.GetModuleName;

public sealed record GetModuleNameQuery(Guid Id, Guid TenantId) : IQuery<ModuleNameDto>;
