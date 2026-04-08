# Phase 04 — DNNN Backend Domain

## Context Links
- SRS Analysis: `plans/reports/srs-analysis-260408-1527-gsdt-v12-delta.md` (section 9, T24-T25)
- PPP BE (reuse patterns): Phase 02
- PppProject entity: `src/modules/investment-projects/GSDT.InvestmentProjects.Domain/Entities/PppProject.cs` (after Phase 2)
- DesignEstimate (reuse): `src/modules/investment-projects/GSDT.InvestmentProjects.Domain/Entities/DesignEstimate.cs` (created in Phase 2)

## Overview
- **Priority:** P1
- **Status:** Pending
- **Effort:** 3-4 days
- **Blocker:** Phase 1 (catalogs)
- **Description:** Build DNNN project type — TPT child entity with DNNN-specific capital structure (CSH/ODA/TCTD), RegistrationCertificate (GCNĐKĐT), reuse DesignEstimate from PPP, CQRS, controller, migration.

## Key Insights
- DNNN capital structure: Vốn CSH + Vốn vay ODA + Vốn vay TCTD (fundamentally different from TN and PPP)
- GCNĐKĐT is a new entity (registration certificate) — list with multiple records per project
- DNNN has 6 tabs (no dedicated revenue tab like PPP)
- Tab1 has KKT/KCN/KCX/FTZ/TTTC field in locations — uses ProjectLocation.IndustrialZoneName (added in Phase 2)
- DesignEstimate (TKTT popup): fully reuse from Phase 2 — FK is to base InvestmentProject
- DNNN "Nghĩa vụ tài chính": DEFERRED — no field spec in SRS, placeholder only
- DNNN Tab2 (HĐ NĐT): reuses InvestorSelection pattern from PPP, minus contract info
- THTH tab: assumed similar to PPP Tab3 with DNNN capital structure
  - **⚠️ BA CONFIRMATION REQUIRED:** DNNN THTH tab has no field spec table in SRS v1.2. Current plan assumes similar to PPP Tab3 with DNNN capital structure. Implement PPP Tab3 first, then adapt for DNNN. Do NOT start DNNN THTH implementation until PPP Tab3 is verified working.

## Requirements

### Functional
1. DnnnProject TPT child entity with DNNN-specific fields
2. DnnnInvestmentDecision with DNNN capital breakdown (CSH/ODA/TCTD)
3. RegistrationCertificate (GCNĐKĐT): multi-record list per project
4. InvestorSelection reuse from PPP (already FK to InvestmentProject base — verify)
5. DesignEstimate reuse from PPP (already FK to InvestmentProject base)
6. Extend ProjectType enum: add Dnnn=4
7. CRUD API: `api/v1/dnnn-projects` + sub-entity endpoints

### Non-Functional
- Same patterns as PPP BE: decimal(18,4), RowVersion, tenant isolation
- No new shared entities needed — only DNNN-specific + reuse

## Architecture

### Entity Model

#### DnnnProject (TPT child of InvestmentProject)

```csharp
public sealed class DnnnProject : InvestmentProject
{
    // DNNN-specific fields (Tab 1)
    string? InvestorName                    // NĐT textbox, max 500
    decimal? StateOwnershipRatio            // Tỷ lệ vốn NN nắm giữ (%), precision(5,2)
    Guid? CompetentAuthorityId              // CQCQ — ref GovernmentAgency catalog
    Guid StatusId                           // MasterData ref: project status
    Guid ProjectGroupId                     // MasterData ref
    SubProjectType SubProjectType           // reuse existing enum
    string? Objective                       // Mục tiêu, max 2000

    // Preliminary capital (DNNN structure)
    decimal PrelimTotalInvestment            // TMĐT
    decimal PrelimEquityCapital             // Vốn CSH
    decimal PrelimOdaLoanCapital            // Vốn vay ODA
    decimal PrelimCreditLoanCapital         // Vốn vay TCTD
    
    // Scale fields
    decimal? AreaHectares
    string? Capacity                        // max 500
    string? MainItems                       // max 2000
    string? ImplementationTimeline          // Thời gian thực hiện, max 200
    string? ProgressDescription             // Tiến độ, max 1000

    // Stop/suspension (same pattern as PPP)
    string? StopContent
    string? StopDecisionNumber
    DateTime? StopDecisionDate
    Guid? StopFileId

    // Navigation — DNNN-specific children
    ICollection<DnnnInvestmentDecision> InvestmentDecisions
    ICollection<RegistrationCertificate> RegistrationCertificates
    ICollection<DesignEstimate> DesignEstimates     // Reuse from Phase 2
    InvestorSelection? InvestorSelection             // Reuse from Phase 2
}
```

#### DnnnInvestmentDecision (Tab 1 — QĐ ĐT)

```csharp
public sealed class DnnnInvestmentDecision : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId, ProjectId
    InvestmentDecisionType DecisionType     // Initial/Adjustment
    string DecisionNumber                   // max 100
    DateTime DecisionDate
    string DecisionAuthority                // max 200
    string? DecisionPerson                  // Người ký, max 200

    // Capital breakdown — DNNN structure
    decimal TotalInvestment                 // TMĐT
    decimal EquityCapital                   // Vốn CSH
    decimal OdaLoanCapital                  // Vốn vay ODA
    decimal CreditLoanCapital               // Vốn vay TCTD
    decimal? EquityRatio                    // Tỷ lệ CSH (auto-calc = CSH/TMĐT)

    Guid? AdjustmentContentId
    string? Notes
    Guid? FileId
    byte[] RowVersion

    DnnnProject Project
}
```

#### RegistrationCertificate (Tab 1 — GCNĐKĐT)

```csharp
public sealed class RegistrationCertificate : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId, ProjectId
    string CertificateNumber                // Số GCN, max 100
    DateTime IssuedDate                     // Ngày cấp
    Guid? FileId                            // File ref
    decimal InvestmentCapital               // Vốn đầu tư
    decimal EquityCapital                   // Vốn CSH
    decimal? EquityRatio                    // Tỷ lệ CSH (auto-calc)
    string? Notes
    byte[] RowVersion

    InvestmentProject Project               // FK to base — reusable by NĐT/FDI
}
```

**Note:** RegistrationCertificate FK is to `InvestmentProject` base (not DnnnProject) so NĐT and FDI can reuse it in Phase 6.

### InvestorSelection Reuse Verification

InvestorSelection was created in Phase 2 with FK to InvestmentProject base (ProjectId as shared PK). DNNN can create an InvestorSelection row for a DnnnProject ID. **No new entity needed** — just add navigation property to DnnnProject and wire up in EF config.

**Confirmed:** InvestorSelection FK targets base `InvestmentProject` (not PppProject) — confirmed reusable for DNNN. No migration change needed. Phase 2 plan explicitly sets `FK → InvestmentProject (base)` with junction table `InvestorSelectionInvestor` for multi-investor storage.

### API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `api/v1/dnnn-projects` | BTC,CQCQ,CDT | List with filters |
| GET | `api/v1/dnnn-projects/{id}` | BTC,CQCQ,CDT | Detail |
| POST | `api/v1/dnnn-projects` | BTC,CDT | Create |
| PUT | `api/v1/dnnn-projects/{id}` | BTC,CDT | Update |
| DELETE | `api/v1/dnnn-projects/{id}` | BTC,CDT | Soft delete |
| POST | `api/v1/dnnn-projects/{id}/locations` | BTC,CDT | Add location |
| DELETE | `api/v1/dnnn-projects/{id}/locations/{lid}` | BTC,CDT | Remove location |
| POST | `api/v1/dnnn-projects/{id}/decisions` | BTC,CDT | Add DNNN decision |
| DELETE | `api/v1/dnnn-projects/{id}/decisions/{did}` | BTC,CDT | Remove decision |
| POST | `api/v1/dnnn-projects/{id}/certificates` | BTC,CDT | Add GCNĐKĐT |
| PUT | `api/v1/dnnn-projects/{id}/certificates/{cid}` | BTC,CDT | Update certificate |
| DELETE | `api/v1/dnnn-projects/{id}/certificates/{cid}` | BTC,CDT | Remove certificate |
| PUT | `api/v1/dnnn-projects/{id}/investor-selection` | BTC,CDT | Upsert investor selection (reuse) |
| POST | `api/v1/dnnn-projects/{id}/design-estimates` | BTC,CDT | Add TKTT (reuse) |
| PUT | `api/v1/dnnn-projects/{id}/design-estimates/{deid}` | BTC,CDT | Update TKTT |
| DELETE | `api/v1/dnnn-projects/{id}/design-estimates/{deid}` | BTC,CDT | Remove TKTT |
| POST | `api/v1/dnnn-projects/{id}/bid-packages` | BTC,CDT | Add bid package (reuse) |
| DELETE | `api/v1/dnnn-projects/{id}/bid-packages/{bid}` | BTC,CDT | Remove bid package |
| POST | `api/v1/dnnn-projects/{id}/documents` | BTC,CDT | Add document (reuse) |
| DELETE | `api/v1/dnnn-projects/{id}/documents/{docid}` | BTC,CDT | Remove document |

## Related Code Files

### CREATE

| File | Description |
|------|-------------|
| `Domain/Entities/DnnnProject.cs` | TPT child entity |
| `Domain/Entities/DnnnInvestmentDecision.cs` | QĐ ĐT with DNNN capital |
| `Domain/Entities/RegistrationCertificate.cs` | GCNĐKĐT (shared base FK) |
| `Application/Commands/CreateDnnnProject/CreateDnnnProjectCommand.cs` | Command |
| `Application/Commands/CreateDnnnProject/CreateDnnnProjectCommandHandler.cs` | Handler |
| `Application/Commands/CreateDnnnProject/CreateDnnnProjectCommandValidator.cs` | Validator |
| `Application/Commands/UpdateDnnnProject/UpdateDnnnProjectCommand.cs` | Command |
| `Application/Commands/UpdateDnnnProject/UpdateDnnnProjectCommandHandler.cs` | Handler |
| `Application/Commands/UpdateDnnnProject/UpdateDnnnProjectCommandValidator.cs` | Validator |
| `Application/Commands/SubEntities/ManageDnnnDecisionCommands.cs` | Decision CRUD |
| `Application/Commands/SubEntities/ManageRegistrationCertificateCommands.cs` | Certificate CRUD |
| `Application/Queries/ListDnnnProjectsQuery.cs` | List with DNNN filters |
| `Application/Queries/GetDnnnProjectByIdQuery.cs` | Detail with all relations |
| `Application/DTOs/DnnnProjectDtos.cs` | Request/response DTOs |
| `Infrastructure/Persistence/Configurations/DnnnProjectConfiguration.cs` | EF config |
| `Presentation/Controllers/DnnnProjectsController.cs` | API controller |

All paths relative to `src/modules/investment-projects/GSDT.InvestmentProjects.{layer}/`

### MODIFY

| File | Change |
|------|--------|
| `Domain/Enums/ProjectType.cs` | Add `Dnnn = 4` |
| `Infrastructure/Persistence/InvestmentProjectsDbContext.cs` | Add DbSets for DnnnProject, DnnnInvestmentDecision, RegistrationCertificate |
| `Infrastructure/Persistence/Configurations/InvestmentProjectConfiguration.cs` | Add RegistrationCertificate nav on base (for NĐT/FDI reuse) |
| `Application/DTOs/SharedDtos.cs` | Add RegistrationCertificateDto |

## Implementation Steps

### Domain Layer (0.5d)

1. Add `Dnnn = 4` to `ProjectType` enum
2. Create `DnnnProject` entity with factory `Create(...)` — set `ProjectType = ProjectType.Dnnn`
3. Create `DnnnInvestmentDecision` entity with factory method
4. Create `RegistrationCertificate` entity with factory method — FK to InvestmentProject base

### Infrastructure Layer (0.5d)

5. Create `DnnnProjectConfiguration` — TPT child table "DnnnProjects"
6. Add DnnnInvestmentDecision configuration
7. Add RegistrationCertificate configuration — add to base InvestmentProject nav (for NĐT/FDI)
8. Add InvestorSelection + DesignEstimate navs to DnnnProject config (reuse existing entities)
9. Register new DbSets in InvestmentProjectsDbContext
10. Generate migration: `dotnet ef migrations add AddDnnnProjectType`

### Application Layer (1.5d)

11. Create `DnnnProjectDtos.cs` — list item, detail, create/update DTOs
12. Add `RegistrationCertificateDto` to SharedDtos
13. Create `CreateDnnnProjectCommand` + Handler + Validator
14. Create `UpdateDnnnProjectCommand` + Handler + Validator
15. Create `ManageDnnnDecisionCommands.cs` — Add/Delete with DNNN capital validation (Total == CSH + ODA + TCTD)
16. Create `ManageRegistrationCertificateCommands.cs` — Add/Update/Delete
17. Create `ListDnnnProjectsQuery` — filters: tên, CQCQ, NĐT, tình trạng, địa điểm
18. Create `GetDnnnProjectByIdQuery` — eager-load all including certificates, investor selection, design estimates

### Presentation Layer (0.5d)

19. Create `DnnnProjectsController` — all endpoints from API table
20. Reuse existing sub-entity command handlers where possible (bid packages, documents, locations, investor selection, design estimates)

### Compile & Test (0.5d)

21. Run `dotnet build`
22. Apply migration
23. Smoke-test CRUD via Swagger

## Todo Checklist

- [ ] ProjectType enum: add Dnnn=4
- [ ] DnnnProject entity + factory
- [ ] DnnnInvestmentDecision entity
- [ ] RegistrationCertificate entity (FK to base)
- [ ] DnnnProjectConfiguration (EF)
- [ ] RegistrationCertificate EF config
- [ ] Wire InvestorSelection + DesignEstimate reuse in DNNN config
- [ ] DbContext: register DbSets
- [ ] EF migration: AddDnnnProjectType
- [ ] DnnnProjectDtos + RegistrationCertificateDto
- [ ] CreateDnnnProject command/handler/validator
- [ ] UpdateDnnnProject command/handler/validator
- [ ] ManageDnnnDecisionCommands
- [ ] ManageRegistrationCertificateCommands
- [ ] ListDnnnProjectsQuery (5 filters)
- [ ] GetDnnnProjectByIdQuery
- [ ] DnnnProjectsController
- [ ] Compile check
- [ ] Migration applies cleanly

## Success Criteria
- `POST /api/v1/dnnn-projects` creates DNNN project with ProjectType=Dnnn
- `GET /api/v1/dnnn-projects/{id}` returns DNNN-specific data: certificates, DNNN decisions (CSH/ODA/TCTD), investor selection, design estimates
- DNNN capital validation: Total == CSH + ODA + TCTD
- RegistrationCertificate CRUD works with FK to base InvestmentProject
- InvestorSelection and DesignEstimate work for DNNN projects (reuse from PPP)
- Migration creates ~3 new tables (DnnnProjects, DnnnInvestmentDecisions, RegistrationCertificates)

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| InvestorSelection FK | Resolved | N/A | FK targets base InvestmentProject — confirmed in Phase 2 plan. No refactor needed. |
| DesignEstimate reuse assumes base FK | Low | Medium | Already specified in Phase 2 plan — verify |
| DNNN THTH tab has no dedicated field table in SRS | Medium | Medium | BA confirmation required before implementing. Implement PPP Tab3 first, adapt for DNNN afterward. Block until confirmed. |
| "Nghĩa vụ TC" deferred — may need adding later | Low | Low | Document as known gap; placeholder text field if needed |

## Security Considerations
- Same auth pattern: BTC/CQCQ/CDT read, BTC/CDT write
- IDOR: verify project ownership on all sub-entity mutations
- Tenant isolation on all queries
