using GSDT.ModuleName.Domain.Entities;
using GSDT.ModuleName.Domain.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace GSDT.ModuleName.Infrastructure.Persistence.Repositories;

public sealed class ModuleNameRepository(ModuleNameDbContext db) : IModuleNameRepository
{
    public async Task<Result<ModuleNameEntity>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.ModuleNames.FirstOrDefaultAsync(e => e.Id == id, ct);
        return entity is not null
            ? Result.Ok(entity)
            : Result.Fail($"ModuleName '{id}' not found.");
    }

    public async Task AddAsync(ModuleNameEntity entity, CancellationToken ct = default)
    {
        await db.ModuleNames.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ModuleNameEntity entity, CancellationToken ct = default)
    {
        db.ModuleNames.Update(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.ModuleNames.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is not null)
        {
            entity.SoftDelete();
            await db.SaveChangesAsync(ct);
        }
    }
}
