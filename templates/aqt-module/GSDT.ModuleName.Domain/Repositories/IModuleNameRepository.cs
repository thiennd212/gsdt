using GSDT.ModuleName.Domain.Entities;
using FluentResults;

namespace GSDT.ModuleName.Domain.Repositories;

public interface IModuleNameRepository
{
    Task<Result<ModuleNameEntity>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ModuleNameEntity entity, CancellationToken ct = default);
    Task UpdateAsync(ModuleNameEntity entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
