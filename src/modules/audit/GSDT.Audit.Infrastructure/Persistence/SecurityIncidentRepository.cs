
namespace GSDT.Audit.Infrastructure.Persistence;

public sealed class SecurityIncidentRepository(AuditDbContext db) : ISecurityIncidentRepository
{
    public async Task AddAsync(SecurityIncident incident, CancellationToken cancellationToken = default)
    {
        db.SecurityIncidents.Add(incident);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<SecurityIncident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.SecurityIncidents.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
