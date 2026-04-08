namespace GSDT.InvestmentProjects.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the investment schema.
/// Uses TPT (Table Per Type) for InvestmentProject hierarchy:
///   Base table  → investment.InvestmentProjects
///   Child table → investment.DomesticProjects
///   Child table → investment.OdaProjects
/// Entity configurations are split across Configurations/ files (each &lt;200 lines).
/// </summary>
public sealed class InvestmentProjectsDbContext(
    DbContextOptions<InvestmentProjectsDbContext> options,
    ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    protected override string SchemaName => "investment";

    // TPT hierarchy
    public DbSet<InvestmentProject> InvestmentProjects => Set<InvestmentProject>();
    public DbSet<DomesticProject> DomesticProjects => Set<DomesticProject>();
    public DbSet<OdaProject> OdaProjects => Set<OdaProject>();
    public DbSet<PppProject> PppProjects => Set<PppProject>();

    // Shared project children
    public DbSet<ProjectLocation> ProjectLocations => Set<ProjectLocation>();
    public DbSet<BidPackage> BidPackages => Set<BidPackage>();
    public DbSet<BidItem> BidItems => Set<BidItem>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<InspectionRecord> InspectionRecords => Set<InspectionRecord>();
    public DbSet<EvaluationRecord> EvaluationRecords => Set<EvaluationRecord>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();
    public DbSet<ViolationRecord> ViolationRecords => Set<ViolationRecord>();
    public DbSet<OperationInfo> OperationInfos => Set<OperationInfo>();
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();

    // Domestic-specific children
    public DbSet<DomesticInvestmentDecision> DomesticInvestmentDecisions => Set<DomesticInvestmentDecision>();
    public DbSet<DomesticCapitalPlan> DomesticCapitalPlans => Set<DomesticCapitalPlan>();
    public DbSet<DomesticExecutionRecord> DomesticExecutionRecords => Set<DomesticExecutionRecord>();
    public DbSet<DomesticDisbursementRecord> DomesticDisbursementRecords => Set<DomesticDisbursementRecord>();

    // ODA-specific children
    public DbSet<OdaInvestmentDecision> OdaInvestmentDecisions => Set<OdaInvestmentDecision>();
    public DbSet<OdaCapitalPlan> OdaCapitalPlans => Set<OdaCapitalPlan>();
    public DbSet<OdaExecutionRecord> OdaExecutionRecords => Set<OdaExecutionRecord>();
    public DbSet<OdaDisbursementRecord> OdaDisbursementRecords => Set<OdaDisbursementRecord>();
    public DbSet<LoanAgreement> LoanAgreements => Set<LoanAgreement>();
    public DbSet<ServiceBank> ServiceBanks => Set<ServiceBank>();
    public DbSet<ProcurementCondition> ProcurementConditions => Set<ProcurementCondition>();

    // PPP-specific children
    public DbSet<PppInvestmentDecision> PppInvestmentDecisions => Set<PppInvestmentDecision>();
    public DbSet<PppCapitalPlan> PppCapitalPlans => Set<PppCapitalPlan>();
    public DbSet<PppExecutionRecord> PppExecutionRecords => Set<PppExecutionRecord>();
    public DbSet<PppDisbursementRecord> PppDisbursementRecords => Set<PppDisbursementRecord>();
    public DbSet<PppContractInfo> PppContractInfos => Set<PppContractInfo>();
    public DbSet<RevenueReport> RevenueReports => Set<RevenueReport>();

    // Cross-type shared entities (PPP + DNNN)
    public DbSet<InvestorSelection> InvestorSelections => Set<InvestorSelection>();
    public DbSet<InvestorSelectionInvestor> InvestorSelectionInvestors => Set<InvestorSelectionInvestor>();
    public DbSet<DesignEstimate> DesignEstimates => Set<DesignEstimate>();
    public DbSet<DesignEstimateItem> DesignEstimateItems => Set<DesignEstimateItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base applies: schema, ApplyConfigurationsFromAssembly, ApplyGlobalFilters, ConfigureOutbox.
        // ApplyGlobalFilters iterates all entity types including TPT derived types.
        // EF Core does NOT allow query filters on derived (non-root) entity types in TPT.
        // Fix: remove filters from DomesticProject and OdaProject after base runs.
        base.OnModelCreating(modelBuilder);

        // Remove query filter from TPT derived types — inherited from InvestmentProject root.
        modelBuilder.Entity<DomesticProject>().HasQueryFilter(null!);
        modelBuilder.Entity<OdaProject>().HasQueryFilter(null!);
        modelBuilder.Entity<PppProject>().HasQueryFilter(null!);
    }
}
