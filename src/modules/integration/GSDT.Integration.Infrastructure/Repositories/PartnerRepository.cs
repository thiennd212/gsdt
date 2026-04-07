
namespace GSDT.Integration.Infrastructure.Repositories;

public sealed class PartnerRepository(IntegrationDbContext dbContext) : IPartnerRepository
{
    public async Task<Partner?> GetByIdAsync(Guid id, CancellationToken ct)
        => await dbContext.Partners.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Partner partner, CancellationToken ct)
    {
        dbContext.Partners.Add(partner);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Partner partner, CancellationToken ct)
    {
        if (dbContext.Entry(partner).State == EntityState.Detached)
            dbContext.Partners.Update(partner);
        await dbContext.SaveChangesAsync(ct);
    }
}
