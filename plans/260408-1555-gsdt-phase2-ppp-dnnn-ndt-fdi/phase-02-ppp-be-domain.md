# Phase 02 — PPP Backend Domain

## Context Links
- SRS Analysis: `plans/reports/srs-analysis-260408-1527-gsdt-v12-delta.md` (sections 8-9)
- Base entity: `src/modules/investment-projects/GSDT.InvestmentProjects.Domain/Entities/InvestmentProject.cs`
- DomesticProject reference: `src/modules/investment-projects/GSDT.InvestmentProjects.Domain/Entities/DomesticProject.cs`
- Controller pattern: `src/modules/investment-projects/GSDT.InvestmentProjects.Presentation/Controllers/DomesticProjectsController.cs`
- EF config: `src/modules/investment-projects/GSDT.InvestmentProjects.Infrastructure/Persistence/Configurations/InvestmentProjectConfiguration.cs`

## Overview
- **Priority:** P1
- **Status:** Complete
- **Effort:** 5-6 days (actual)
- **Commit:** ca0383b
- **Date:** 2026-04-08
- **Blocker:** Phase 1 (catalogs must exist for GovernmentAgency/Investor refs)
- **Description:** Built PPP project type — TPT child entity, 10 sub-entities, CQRS commands/queries, 25+ API endpoints, EF migration. 100% TodoChecklist completion.

## Key Insights
- PPP is the most complex new type: 7 tabs, unique Tab2 (contract info), unique Tab6 (revenue)
- Capital structure: Vốn NN (NSTW+NSĐP+NSNN khác) + CSH + Vay — different from both TN and ODA
- DesignEstimate is shared with DNNN — build generically now, reuse in Phase 4
- InvestorSelection references Investor catalog (Phase 1) via multiselect Guid[]
- PPP Tab2 (HĐ dự án) is entirely new — no P1 equivalent, needs careful modeling
- Loại hợp đồng: cascading enum — BOT | BT (→ sub-types) | Khác (→ sub-types)

## Requirements

### Functional
1. PppProject TPT child entity with PPP-specific fields (contract type, preparation unit)
2. PppInvestmentDecision with PPP capital breakdown (VonNN/CSH/Vay)
3. PppCapitalPlan for "KH bố trí vốn NN tham gia"
4. PppDisbursementRecord with 3-source disbursement (VonNN/CSH/Vay)
5. PppExecutionRecord with sub-project state capital fields
6. InvestorSelection (1-to-1 with PppProject) — investor multiselect + QĐ
7. PppContractInfo (1-to-1 with PppProject) — TMĐT breakdown + tiến độ + HĐ ký kết
8. RevenueReport[] — periodic revenue reporting (PPP-only)
9. DesignEstimate[] — TKTT popup with 7 cost items + hạng mục (shared with DNNN)
10. Extend ProjectType enum: add Ppp=3
11. CRUD API: `api/v1/ppp-projects` + all sub-entity endpoints

### Non-Functional
- All money fields: `decimal` precision (18,4)
- RowVersion on all mutable entities
- Concurrency via optimistic locking (ETag/RowVersion)
- Tenant isolation on all queries

## Architecture

### Entity Model

#### PppProject (TPT child of InvestmentProject)

```csharp
public sealed class PppProject : InvestmentProject
{
    // PPP-specific fields (Tab 1)
    PppContractType ContractType            // enum: BOT=1,BT_Land=2,BT_Money=3,BT_NoPayment=4,BTO=5,BOO=6,OM=7,BTL=8,BLT=9,Mixed=10
    SubProjectType SubProjectType           // reuse existing enum
    Guid ProjectGroupId                     // MasterData ref
    Guid StatusId                           // MasterData ref: project status
    string? PreparationUnit                 // "NĐT đề xuất" or "CQ thẩm quyền lập"
    Guid? CompetentAuthorityId              // CQCQ — ref GovernmentAgency catalog
    string? Objective                       // Mục tiêu, max 2000
    
    // Preliminary capital (SBTMLT)
    decimal PrelimTotalInvestment            // Sơ bộ TMĐT
    decimal PrelimStateCapital              // Vốn NN
    decimal PrelimEquityCapital             // CSH
    decimal PrelimLoanCapital               // Vay & huy động

    // Scale fields
    decimal? AreaHectares                   // Diện tích (ha)
    string? Capacity                        // Công suất, max 500
    string? MainItems                       // Hạng mục chính, max 2000

    // Stop/suspension
    string? StopContent
    string? StopDecisionNumber
    DateTime? StopDecisionDate
    Guid? StopFileId

    // Navigation — PPP-specific children
    ICollection<PppInvestmentDecision> InvestmentDecisions
    ICollection<PppCapitalPlan> CapitalPlans
    ICollection<PppExecutionRecord> ExecutionRecords
    ICollection<PppDisbursementRecord> DisbursementRecords
    ICollection<DesignEstimate> DesignEstimates
    ICollection<RevenueReport> RevenueReports
    InvestorSelection? InvestorSelection      // 1-to-1
    PppContractInfo? ContractInfo             // 1-to-1
}
```

#### PppInvestmentDecision (Tab 1 — QĐ ĐT)

```csharp
public sealed class PppInvestmentDecision : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId, ProjectId
    InvestmentDecisionType DecisionType     // Initial/Adjustment (reuse existing enum)
    string DecisionNumber                   // max 100
    DateTime DecisionDate
    string DecisionAuthority                // max 200
    string? DecisionPerson                  // Người ký, max 200
    
    // Capital breakdown — PPP structure
    decimal TotalInvestment                 // TMĐT
    decimal StateCapital                    // Tổng vốn NN
    decimal CentralBudget                   // NSTW
    decimal LocalBudget                     // NSĐP
    decimal OtherStateBudget               // NSNN khác
    decimal EquityCapital                   // Vốn CSH
    decimal? EquityRatio                    // Tỷ lệ CSH (auto-calc = CSH/TMĐT)
    decimal LoanCapital                     // Vốn vay & huy động
    
    Guid? AdjustmentContentId              // MasterData ref
    string? Notes
    Guid? FileId
    byte[] RowVersion
    
    PppProject Project                     // Navigation
}
```

#### InvestorSelection (Tab 2 — 1-to-1 with PppProject)

```csharp
public sealed class InvestorSelection : AuditableEntity<Guid>, ITenantScoped
{
    Guid ProjectId                          // PK + FK → InvestmentProject (base), shared PK pattern
    Guid TenantId
    string? SelectionMethod                 // "Đấu thầu rộng rãi", "Đàm phán", "Chỉ định", "Khác"
    string? SelectionDecisionNumber         // QĐ lựa chọn, max 100
    DateTime? SelectionDecisionDate
    Guid? SelectionFileId                   // File ref
    byte[] RowVersion
    
    // FK → InvestmentProject (base), NOT PppProject — allows DNNN reuse without migration change
    InvestmentProject Project
    
    // Investors: junction table (NOT Guid[] jsonb) — SQL Server referential integrity
    ICollection<InvestorSelectionInvestor> Investors
}

// Junction table: InvestorSelection ↔ Investor (replaces Guid[] jsonb)
public sealed class InvestorSelectionInvestor
{
    Guid InvestorSelectionId               // FK → InvestorSelection, composite PK
    Guid InvestorId                        // FK → Investor catalog, composite PK
    int SortOrder                          // Display order of selected investors
}
```
```

#### PppContractInfo (Tab 2 — 1-to-1 with PppProject)

```csharp
public sealed class PppContractInfo : AuditableEntity<Guid>, ITenantScoped
{
    Guid ProjectId                          // PK + FK
    Guid TenantId
    
    // TMĐT breakdown (same as QĐ ĐT but for contract)
    decimal TotalInvestment
    decimal StateCapital
    decimal CentralBudget                   // NSTW
    decimal LocalBudget                     // NSĐP
    decimal OtherStateBudget               // NSNN khác
    decimal EquityCapital
    decimal? EquityRatio                    // auto-calc
    decimal LoanCapital
    
    // Tiến độ
    string? ImplementationProgress          // max 1000
    string? ContractDuration                // "10 năm", max 200
    string? RevenueSharingMechanism         // Cơ chế chia sẻ tăng/giảm, max 2000
    
    // HĐ ký kết
    string? ContractAuthority               // Cơ quan ký HĐ, max 200
    string? ContractNumber                  // max 100
    DateTime? ContractDate
    DateTime? ConstructionStartDate         // Thời gian khởi công
    DateTime? CompletionDate                // Thời gian hoàn thành
    
    byte[] RowVersion
    PppProject Project
}
```

#### PppCapitalPlan (Tab 3 — KH bố trí vốn NN)

```csharp
public sealed class PppCapitalPlan : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId, ProjectId
    int AllocationRound                     // Lần cấp vốn (1-20)
    decimal StateCapitalByDecision          // Số vốn NN theo QĐ
    string? Notes
    byte[] RowVersion
    
    PppProject Project
}
```

#### PppDisbursementRecord (Tab 4 — 3-source)

```csharp
public sealed class PppDisbursementRecord : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId, ProjectId
    DateTime ReportDate
    
    // Vốn nhà nước
    decimal StateCapitalPeriod              // Giải ngân kỳ
    decimal StateCapitalCumulative          // Lũy kế
    // Vốn CSH
    decimal EquityCapitalPeriod
    decimal EquityCapitalCumulative
    // Vốn vay
    decimal LoanCapitalPeriod
    decimal LoanCapitalCumulative
    
    byte[] RowVersion
    PppProject Project
}
```

#### PppExecutionRecord (Tab 3 — THTH)

```csharp
public sealed class PppExecutionRecord : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId, ProjectId
    DateTime ReportDate
    
    // Same base fields as DomesticExecutionRecord
    string? OverallProgress                 // Tổng tiến độ
    string? Issues                          // Khó khăn vướng mắc
    string? Recommendations                 // Kiến nghị
    
    // PPP-specific: sub-project state capital fields
    decimal? SubProjectStateCapitalApproved
    decimal? SubProjectStateCapitalDisbursed
    decimal? CumulativeFromConstruction     // Lũy kế từ khởi công
    
    byte[] RowVersion
    PppProject Project
}
```

#### DesignEstimate (Shared PPP+DNNN — TKTT popup)

```csharp
public sealed class DesignEstimate : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId, ProjectId               // FK to InvestmentProject (base)
    
    // QĐ phê duyệt
    string? ApprovalDecisionNumber          // max 100
    DateTime? ApprovalDecisionDate
    string? ApprovalAuthority               // max 200
    Guid? ApprovalFileId
    
    // Dự toán — 7 cost items (precision 18,4)
    decimal EquipmentCost                   // Thiết bị
    decimal ConstructionCost                // Xây lắp
    decimal LandCompensationCost            // GPMB
    decimal ManagementCost                  // QLDA
    decimal ConsultancyCost                 // Tư vấn
    decimal ContingencyCost                 // Dự phòng
    decimal OtherCost                       // Khác
    decimal TotalEstimate                   // Auto-sum (enforced in handler — SERVER IS AUTHORITATIVE. Always recompute on save, ignore client-sent value.)
    
    string? Notes
    byte[] RowVersion
    
    InvestmentProject Project               // Nav to BASE (not PppProject) — allows DNNN reuse
}
```

#### DesignEstimateItem (child of DesignEstimate — hạng mục detail)

```csharp
public sealed class DesignEstimateItem : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId
    Guid DesignEstimateId
    string ItemName                         // Tên hạng mục, max 500
    decimal EstimatedAmount                 // Giá trị dự toán
    string? Notes
    
    DesignEstimate DesignEstimate           // Navigation
}
```

#### RevenueReport (PPP Tab 6 — periodic)

```csharp
public sealed class RevenueReport : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId, ProjectId
    int ReportYear                          // Năm báo cáo (current-5..current)
    string ReportPeriod                     // "6 tháng" | "1 năm"
    decimal RevenuePeriod                   // Doanh thu trong kỳ (triệu VNĐ)
    decimal RevenueCumulative               // Lũy kế doanh thu
    string? RevenueIncreaseSharing          // Chia sẻ tăng doanh thu
    string? RevenueDecreaseSharing          // Chia sẻ giảm doanh thu
    string? Difficulties                    // Khó khăn vướng mắc
    string? Recommendations                 // Kiến nghị
    byte[] RowVersion
    
    PppProject Project
}
```

### New Enums

```csharp
// PppContractType.cs
public enum PppContractType
{
    BOT = 1,
    BT_Land = 2,          // BT thanh toán bằng đất
    BT_Money = 3,         // BT thanh toán bằng tiền
    BT_NoPayment = 4,     // BT không thanh toán
    BTO = 5,
    BOO = 6,
    OM = 7,               // O&M
    BTL = 8,
    BLT = 9,
    Mixed = 10            // HĐ hỗn hợp
}
```

### API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `api/v1/ppp-projects` | BTC,CQCQ,CDT | List with filters |
| GET | `api/v1/ppp-projects/{id}` | BTC,CQCQ,CDT | Detail (all tabs data) |
| POST | `api/v1/ppp-projects` | BTC,CDT | Create |
| PUT | `api/v1/ppp-projects/{id}` | BTC,CDT | Update |
| DELETE | `api/v1/ppp-projects/{id}` | BTC,CDT | Soft delete |
| POST | `api/v1/ppp-projects/{id}/locations` | BTC,CDT | Add location |
| DELETE | `api/v1/ppp-projects/{id}/locations/{lid}` | BTC,CDT | Remove location |
| POST | `api/v1/ppp-projects/{id}/decisions` | BTC,CDT | Add investment decision |
| DELETE | `api/v1/ppp-projects/{id}/decisions/{did}` | BTC,CDT | Remove decision |
| PUT | `api/v1/ppp-projects/{id}/investor-selection` | BTC,CDT | Upsert investor selection |
| PUT | `api/v1/ppp-projects/{id}/contract-info` | BTC,CDT | Upsert contract info |
| POST | `api/v1/ppp-projects/{id}/capital-plans` | BTC,CDT | Add capital plan |
| DELETE | `api/v1/ppp-projects/{id}/capital-plans/{cid}` | BTC,CDT | Remove capital plan |
| POST | `api/v1/ppp-projects/{id}/bid-packages` | BTC,CDT | Add bid package (reuse) |
| DELETE | `api/v1/ppp-projects/{id}/bid-packages/{bid}` | BTC,CDT | Remove bid package |
| POST | `api/v1/ppp-projects/{id}/design-estimates` | BTC,CDT | Add design estimate |
| PUT | `api/v1/ppp-projects/{id}/design-estimates/{deid}` | BTC,CDT | Update design estimate |
| DELETE | `api/v1/ppp-projects/{id}/design-estimates/{deid}` | BTC,CDT | Remove design estimate |
| POST | `api/v1/ppp-projects/{id}/execution-records` | BTC,CDT | Add execution record |
| POST | `api/v1/ppp-projects/{id}/disbursements` | BTC,CDT | Add disbursement |
| DELETE | `api/v1/ppp-projects/{id}/disbursements/{did}` | BTC,CDT | Remove disbursement |
| POST | `api/v1/ppp-projects/{id}/revenue-reports` | BTC,CDT | Add revenue report |
| PUT | `api/v1/ppp-projects/{id}/revenue-reports/{rid}` | BTC,CDT | Update revenue report |
| DELETE | `api/v1/ppp-projects/{id}/revenue-reports/{rid}` | BTC,CDT | Remove revenue report |
| POST | `api/v1/ppp-projects/{id}/documents` | BTC,CDT | Add document (reuse) |
| DELETE | `api/v1/ppp-projects/{id}/documents/{docid}` | BTC,CDT | Remove document |

## Related Code Files

### CREATE

| File | Description |
|------|-------------|
| `Domain/Entities/PppProject.cs` | TPT child entity |
| `Domain/Entities/PppInvestmentDecision.cs` | QĐ ĐT with PPP capital |
| `Domain/Entities/InvestorSelection.cs` | Tab2 NĐT selection (1-to-1) |
| `Domain/Entities/PppContractInfo.cs` | Tab2 contract details (1-to-1) |
| `Domain/Entities/PppCapitalPlan.cs` | KH bố trí vốn NN |
| `Domain/Entities/PppDisbursementRecord.cs` | 3-source disbursement |
| `Domain/Entities/PppExecutionRecord.cs` | THTH records |
| `Domain/Entities/DesignEstimate.cs` | TKTT popup (shared) |
| `Domain/Entities/DesignEstimateItem.cs` | TKTT hạng mục child |
| `Domain/Entities/RevenueReport.cs` | PPP revenue reporting |
| `Domain/Enums/PppContractType.cs` | 10-value contract type enum |
| `Application/Commands/CreatePppProject/CreatePppProjectCommand.cs` | Command |
| `Application/Commands/CreatePppProject/CreatePppProjectCommandHandler.cs` | Handler |
| `Application/Commands/CreatePppProject/CreatePppProjectCommandValidator.cs` | Validator |
| `Application/Commands/UpdatePppProject/UpdatePppProjectCommand.cs` | Command |
| `Application/Commands/UpdatePppProject/UpdatePppProjectCommandHandler.cs` | Handler |
| `Application/Commands/UpdatePppProject/UpdatePppProjectCommandValidator.cs` | Validator |
| `Application/Commands/SubEntities/ManagePppDecisionCommands.cs` | Decision CRUD |
| `Application/Commands/SubEntities/ManagePppContractCommands.cs` | Contract info + investor selection upsert |
| `Application/Commands/SubEntities/ManagePppCapitalPlanCommands.cs` | Capital plan CRUD |
| `Application/Commands/SubEntities/ManagePppDisbursementCommands.cs` | Disbursement CRUD |
| `Application/Commands/SubEntities/ManagePppExecutionCommands.cs` | Execution CRUD |
| `Application/Commands/SubEntities/ManageDesignEstimateCommands.cs` | Design estimate CRUD (shared) |
| `Application/Commands/SubEntities/ManageRevenueReportCommands.cs` | Revenue CRUD |
| `Application/Queries/ListPppProjectsQuery.cs` | List with PPP filters |
| `Application/Queries/GetPppProjectByIdQuery.cs` | Detail with all relations |
| `Application/DTOs/PppProjectDtos.cs` | Request/response DTOs |
| `Infrastructure/Persistence/Configurations/PppProjectConfiguration.cs` | EF config |
| `Presentation/Controllers/PppProjectsController.cs` | API controller |

All paths relative to `src/modules/investment-projects/GSDT.InvestmentProjects.{layer}/`

### MODIFY

| File | Change |
|------|--------|
| `Domain/Enums/ProjectType.cs` | Add `Ppp = 3` |
| `Domain/Entities/ProjectLocation.cs` | Add `string? IndustrialZoneName` (for DNNN, but column exists for all) |
| `Infrastructure/Persistence/Configurations/InvestmentProjectConfiguration.cs` | Add DesignEstimate nav on base |
| `Infrastructure/Persistence/InvestmentProjectsDbContext.cs` | Add DbSets for new entities |
| `Application/DTOs/SharedDtos.cs` | Add DesignEstimateDto, RevenueReportDto |

## Implementation Steps

### Domain Layer (1d)

1. Add `Ppp = 3` to `ProjectType` enum
2. Create `PppContractType` enum (10 values)
3. Create `PppProject` entity with factory `Create(...)` method — set `ProjectType = ProjectType.Ppp`
4. Create `PppInvestmentDecision` entity with factory method
5. Create `InvestorSelection` entity (shared PK pattern like OperationInfo)
6. Create `PppContractInfo` entity (shared PK pattern)
7. Create `PppCapitalPlan` entity
8. Create `PppDisbursementRecord` entity (3-source)
9. Create `PppExecutionRecord` entity
10. Create `DesignEstimate` entity — FK to `InvestmentProject` base (not PppProject) for DNNN reuse
11. Create `DesignEstimateItem` entity (child of DesignEstimate)
12. Create `RevenueReport` entity
13. Add `string? IndustrialZoneName` to `ProjectLocation` (nullable, max 500)

### Infrastructure Layer (1d)

14. Create `PppProjectConfiguration` — TPT child table "PppProjects", all child nav configs
15. Add DesignEstimate + DesignEstimateItem configurations (in new `DesignEstimateConfiguration.cs`)
16. Add RevenueReport configuration
17. Add InvestorSelection + PppContractInfo configurations (shared PK pattern)
18. Add PppInvestmentDecision, PppCapitalPlan, PppDisbursementRecord, PppExecutionRecord configs
19. Update `InvestmentProjectConfiguration` — add DesignEstimate nav on base entity
20. Register all new DbSets in `InvestmentProjectsDbContext`
21. Generate migration: `dotnet ef migrations add AddPppProjectType`

### Application Layer (2d)

22. Create `PppProjectDtos.cs` — list item, detail, create/update request DTOs
23. Add `DesignEstimateDto`, `RevenueReportDto`, `InvestorSelectionDto`, `PppContractInfoDto` to SharedDtos
24. Create `CreatePppProjectCommand` + Handler + Validator — validate code uniqueness, factory Create
25. Create `UpdatePppProjectCommand` + Handler + Validator — load, map, save
26. Create `ManagePppDecisionCommands.cs` — Add/Delete with PPP capital validation (Total == StateCapital + Equity + Loan)
27. Create `ManagePppContractCommands.cs` — UpsertInvestorSelection + UpsertPppContractInfo
28. Create `ManagePppCapitalPlanCommands.cs` — Add/Delete
29. Create `ManagePppDisbursementCommands.cs` — Add/Delete
30. Create `ManagePppExecutionCommands.cs` — Add/Delete
31. Create `ManageDesignEstimateCommands.cs` — Add/Update/Delete with TotalEstimate auto-sum. **Server handler is authoritative for TotalEstimate computation. Always recompute on save (sum of 7 cost items), ignore any client-sent TotalEstimate value.**
32. Create `ManageRevenueReportCommands.cs` — Add/Update/Delete with cumulative auto-calc
33. Create `ListPppProjectsQuery` — paginated list with search + PPP-specific filters (contractType, competentAuthority, location, investmentField)
34. Create `GetPppProjectByIdQuery` — eager-load all relations

### Presentation Layer (0.5d)

35. Create `PppProjectsController` following DomesticProjectsController pattern — all endpoints from API table above

### Validation & Compile (0.5d)

36. Run `dotnet build` — fix compile errors
37. Apply migration to dev database
38. Smoke-test CRUD via Swagger/curl

## Todo Checklist

- [x] ProjectType enum: add Ppp=3
- [x] PppContractType enum
- [x] PppProject entity + factory
- [x] PppInvestmentDecision entity
- [x] InvestorSelection entity (shared PK, FK → InvestmentProject base)
- [x] InvestorSelectionInvestor junction table entity + EF config
- [x] PppContractInfo entity (shared PK)
- [x] PppCapitalPlan entity
- [x] PppDisbursementRecord entity (3-source)
- [x] PppExecutionRecord entity
- [x] DesignEstimate entity (FK to base InvestmentProject)
- [x] DesignEstimateItem entity
- [x] RevenueReport entity
- [x] ProjectLocation: add IndustrialZoneName
- [x] PppProjectConfiguration (EF)
- [x] All sub-entity EF configs
- [x] DbContext: register DbSets
- [x] EF migration: AddPppProjectType
- [x] PppProjectDtos + shared DTOs
- [x] CreatePppProject command/handler/validator
- [x] UpdatePppProject command/handler/validator
- [x] Sub-entity commands (7 command files)
- [x] ListPppProjectsQuery
- [x] GetPppProjectByIdQuery
- [x] PppProjectsController
- [x] Compile check
- [x] Migration applies cleanly

## Success Criteria
- `POST /api/v1/ppp-projects` creates PPP project with ProjectType=Ppp in DB
- `GET /api/v1/ppp-projects/{id}` returns all 7 tabs of data (decisions, investor selection, contract info, capital plans, bid packages, design estimates, disbursements, execution records, revenue reports, inspection/evaluation/audit records, operation info, documents)
- PPP capital validation: TotalInvestment == StateCapital + EquityCapital + LoanCapital
- DesignEstimate TotalEstimate == sum of 7 cost items
- InvestorSelection uses InvestorSelectionInvestor junction table (SQL Server referential integrity, no jsonb)
- EF migration creates ~10 new tables in "investment" schema

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| PPP Tab2 has no P1 pattern — InvestorSelection + PppContractInfo are new | Medium | High | Follow OperationInfo shared-PK pattern, test thoroughly |
| InvestorSelection multi-investor storage | Resolved | N/A | Junction table InvestorSelectionInvestor used — SQL Server referential integrity, no jsonb needed |
| 10 new tables in single migration may be large | Low | Low | Split into 2 migrations if needed (entities + configs separately) |
| DesignEstimate FK to base InvestmentProject — EF TPT query perf | Low | Medium | Include explicit `.Include()` in queries, benchmark |

## Security Considerations
- Same auth pattern as DomesticProjectsController: `[Authorize(Roles = "BTC,CQCQ,CDT")]` for read, `[Authorize(Roles = "BTC,CDT")]` for write
- IDOR prevention: all sub-entity mutations verify ProjectId ownership via tenant-scoped query
- Concurrency: RowVersion on PppProject + all mutable sub-entities

## Next Steps
- Phase 3 (PPP FE) depends on this phase completing with working API
- Phase 4 (DNNN BE) will reuse DesignEstimate entity from this phase
