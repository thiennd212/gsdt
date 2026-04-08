using GSDT.InvestmentProjects.Domain.Repositories;

namespace GSDT.InvestmentProjects.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IInvestmentProjectRepository.
/// Uses TrackingQuery for command handlers (writes) and Query (no-tracking) for existence checks.
/// </summary>
internal sealed class InvestmentProjectRepository(InvestmentProjectsDbContext context)
    : IInvestmentProjectRepository
{
    public async Task<InvestmentProject?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.InvestmentProjects
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<DomesticProject?> GetDomesticByIdWithDetailsAsync(
        Guid id, Guid tenantId, CancellationToken ct = default)
        => await context.DomesticProjects
            .AsNoTracking()
            .Where(p => p.Id == id && p.TenantId == tenantId)
            .Include(p => p.Locations)
            .Include(p => p.InvestmentDecisions)
            .Include(p => p.CapitalPlans)
            .Include(p => p.BidPackages).ThenInclude(bp => bp.BidItems)
            .Include(p => p.BidPackages).ThenInclude(bp => bp.Contracts)
            .Include(p => p.ExecutionRecords)
            .Include(p => p.DisbursementRecords)
            .Include(p => p.InspectionRecords)
            .Include(p => p.EvaluationRecords)
            .Include(p => p.AuditRecords)
            .Include(p => p.ViolationRecords)
            .Include(p => p.OperationInfo)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(ct);

    public async Task<OdaProject?> GetOdaByIdWithDetailsAsync(
        Guid id, Guid tenantId, CancellationToken ct = default)
        => await context.OdaProjects
            .AsNoTracking()
            .Where(p => p.Id == id && p.TenantId == tenantId)
            .Include(p => p.Locations)
            .Include(p => p.InvestmentDecisions)
            .Include(p => p.CapitalPlans)
            .Include(p => p.BidPackages).ThenInclude(bp => bp.BidItems)
            .Include(p => p.BidPackages).ThenInclude(bp => bp.Contracts)
            .Include(p => p.ExecutionRecords)
            .Include(p => p.DisbursementRecords)
            .Include(p => p.InspectionRecords)
            .Include(p => p.EvaluationRecords)
            .Include(p => p.AuditRecords)
            .Include(p => p.ViolationRecords)
            .Include(p => p.OperationInfo)
            .Include(p => p.Documents)
            .Include(p => p.LoanAgreements)
            .Include(p => p.ServiceBanks)
            .Include(p => p.ProcurementCondition)
            .FirstOrDefaultAsync(ct);

    public async Task<InvestmentProject?> GetByIdWithLocationsAsync(Guid id, CancellationToken ct = default)
        => await context.InvestmentProjects
            .Include(p => p.Locations)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<DomesticProject?> GetDomesticByIdWithDecisionsAsync(Guid id, CancellationToken ct = default)
        => await context.DomesticProjects
            .Include(p => p.InvestmentDecisions)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<InvestmentProject?> GetByIdWithBidPackagesAsync(Guid id, CancellationToken ct = default)
        => await context.InvestmentProjects
            .Include(p => p.BidPackages)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<InvestmentProject?> GetByIdWithDocumentsAsync(Guid id, CancellationToken ct = default)
        => await context.InvestmentProjects
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PppProject?> GetPppByIdWithDetailsAsync(
        Guid id, Guid tenantId, CancellationToken ct = default)
        => await context.PppProjects
            .AsNoTracking()
            .Where(p => p.Id == id && p.TenantId == tenantId)
            .Include(p => p.Locations)
            .Include(p => p.InvestmentDecisions)
            .Include(p => p.CapitalPlans)
            .Include(p => p.ExecutionRecords)
            .Include(p => p.DisbursementRecords)
            .Include(p => p.RevenueReports)
            .Include(p => p.ContractInfo)
            .Include(p => p.BidPackages).ThenInclude(bp => bp.BidItems)
            .Include(p => p.BidPackages).ThenInclude(bp => bp.Contracts)
            .Include(p => p.InspectionRecords)
            .Include(p => p.EvaluationRecords)
            .Include(p => p.AuditRecords)
            .Include(p => p.ViolationRecords)
            .Include(p => p.Documents)
            .Include(p => p.InvestorSelection).ThenInclude(s => s!.Investors)
            .Include(p => p.DesignEstimates).ThenInclude(de => de.Items)
            .FirstOrDefaultAsync(ct);

    public async Task<PppProject?> GetPppByIdWithDecisionsAsync(Guid id, CancellationToken ct = default)
        => await context.PppProjects
            .Include(p => p.InvestmentDecisions)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PppProject?> GetPppByIdWithCapitalPlansAsync(Guid id, CancellationToken ct = default)
        => await context.PppProjects
            .Include(p => p.CapitalPlans)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PppProject?> GetPppByIdWithDisbursementsAsync(Guid id, CancellationToken ct = default)
        => await context.PppProjects
            .Include(p => p.DisbursementRecords)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PppProject?> GetPppByIdWithExecutionsAsync(Guid id, CancellationToken ct = default)
        => await context.PppProjects
            .Include(p => p.ExecutionRecords)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PppProject?> GetPppByIdWithRevenueReportsAsync(Guid id, CancellationToken ct = default)
        => await context.PppProjects
            .Include(p => p.RevenueReports)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<InvestmentProject?> GetByIdWithInvestorSelectionAsync(Guid id, CancellationToken ct = default)
        => await context.InvestmentProjects
            .Include(p => p.InvestorSelection).ThenInclude(s => s!.Investors)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PppProject?> GetPppByIdWithContractInfoAsync(Guid id, CancellationToken ct = default)
        => await context.PppProjects
            .Include(p => p.ContractInfo)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<InvestmentProject?> GetByIdWithDesignEstimatesAsync(Guid id, CancellationToken ct = default)
        => await context.InvestmentProjects
            .Include(p => p.DesignEstimates).ThenInclude(de => de.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await context.InvestmentProjects
            .AnyAsync(p => p.Id == id, ct);

    public async Task<bool> ProjectCodeExistsAsync(
        string code,
        Guid tenantId,
        Guid? excludeId = null,
        CancellationToken ct = default)
        => await context.InvestmentProjects
            .AnyAsync(p =>
                p.ProjectCode == code &&
                p.TenantId == tenantId &&
                (excludeId == null || p.Id != excludeId.Value),
                ct);

    public void Add(InvestmentProject project)
        => context.InvestmentProjects.Add(project);

    public void Remove(InvestmentProject project)
        => project.SoftDelete();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
