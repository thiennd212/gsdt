# Phase 06 — NĐT trong nước + FDI (Backend + Frontend)

## Context Links
- SRS Analysis: `plans/reports/srs-analysis-260408-1527-gsdt-v12-delta.md` (section 10)
- DNNN BE/FE: Phases 04-05 (clone source)
- PPP shared entities: Phase 02 (DesignEstimate, InvestorSelection)

## Overview
- **Priority:** P2
- **Status:** Pending
- **Effort:** 3-5 days (NĐT: 2-3d, FDI: 1-2d)
- **Blocker:** Phase 4 (DNNN BE) + Phase 5 (DNNN FE)
- **Description:** NĐT = DNNN clone minus 3 features (TKTT, HĐ NĐT, Nghĩa vụ TC). FDI = NĐT clone with label changes. SRS says "tương tự" for both — no dedicated field tables. ~85% code reuse from DNNN.

## Key Insights
- SRS explicitly states NĐT is "tương tự DNNN" minus 3 functions (R77 TKTT, R78 HĐ NĐT, R80 Nghĩa vụ TC)
- FDI is "tương tự NĐT trong nước" — identical structure
- Both NĐT and FDI keep: GCNĐKĐT, QĐ chấp thuận/điều chỉnh, THTH, Thanh tra, Khai thác, Tài liệu
- Both NĐT and FDI **drop**: DesignEstimate (TKTT), InvestorSelection (HĐ NĐT)
- NĐT/FDI are the thinnest project types — 4 tabs (Tab1 general + Tab2 THTH + Thanh tra + Khai thác + Tài liệu) — effectively 5 tabs
- The clone approach: copy DNNN entity/FE, remove features, adjust labels

## Requirements

### Functional — NĐT
1. NdtProject TPT child entity — same fields as DNNN minus CompetentAuthorityId scope
2. NdtInvestmentDecision — same DNNN capital structure (CSH/ODA/TCTD)
3. RegistrationCertificate — reuse (FK to base InvestmentProject)
4. **No** DesignEstimate (no TKTT popup)
5. **No** InvestorSelection (no HĐ NĐT tab)
6. 5 tabs: Thông tin QĐĐT, THTH, Thanh tra, Khai thác, Tài liệu
7. CRUD API: `api/v1/ndt-projects`

### Functional — FDI
1. FdiProject TPT child entity — identical to NĐT
2. FdiInvestmentDecision — same capital structure
3. RegistrationCertificate — reuse
4. Same 5 tabs as NĐT
5. CRUD API: `api/v1/fdi-projects`

### Non-Functional
- Same patterns as DNNN: decimal(18,4), RowVersion, tenant isolation
- Keep DRY: maximize reuse, minimize code duplication

## Architecture

### Entity Model — NĐT

```csharp
public sealed class NdtProject : InvestmentProject
{
    // Same as DNNN minus nothing structurally — fields are the same
    string? InvestorName                    // max 500
    decimal? StateOwnershipRatio            // %
    Guid? CompetentAuthorityId              // GovernmentAgency ref
    Guid StatusId
    Guid ProjectGroupId
    SubProjectType SubProjectType
    string? Objective                       // max 2000

    // DNNN capital structure (same)
    decimal PrelimTotalInvestment
    decimal PrelimEquityCapital
    decimal PrelimOdaLoanCapital
    decimal PrelimCreditLoanCapital

    // Scale
    decimal? AreaHectares
    string? Capacity
    string? MainItems
    string? ImplementationTimeline
    string? ProgressDescription

    // Stop/suspension
    string? StopContent, StopDecisionNumber
    DateTime? StopDecisionDate
    Guid? StopFileId

    // Navigation — NĐT children (subset of DNNN)
    ICollection<NdtInvestmentDecision> InvestmentDecisions
    ICollection<RegistrationCertificate> RegistrationCertificates  // reuse
    // NO DesignEstimate
    // NO InvestorSelection
}
```

### Entity Model — FDI

```csharp
public sealed class FdiProject : InvestmentProject
{
    // IDENTICAL fields to NdtProject
    string? InvestorName
    decimal? StateOwnershipRatio
    Guid? CompetentAuthorityId
    Guid StatusId
    Guid ProjectGroupId
    SubProjectType SubProjectType
    string? Objective

    decimal PrelimTotalInvestment
    decimal PrelimEquityCapital
    decimal PrelimOdaLoanCapital
    decimal PrelimCreditLoanCapital

    decimal? AreaHectares
    string? Capacity, MainItems, ImplementationTimeline, ProgressDescription

    string? StopContent, StopDecisionNumber
    DateTime? StopDecisionDate
    Guid? StopFileId

    ICollection<FdiInvestmentDecision> InvestmentDecisions
    ICollection<RegistrationCertificate> RegistrationCertificates
}
```

### Decision Entities

```csharp
// NdtInvestmentDecision — identical structure to DnnnInvestmentDecision
public sealed class NdtInvestmentDecision : AuditableEntity<Guid>, ITenantScoped
{
    // Same fields: TenantId, ProjectId, DecisionType, DecisionNumber, DecisionDate,
    // DecisionAuthority, DecisionPerson, TotalInvestment, EquityCapital,
    // OdaLoanCapital, CreditLoanCapital, EquityRatio, AdjustmentContentId, Notes, FileId, RowVersion
    NdtProject Project
}

// FdiInvestmentDecision — identical
public sealed class FdiInvestmentDecision : AuditableEntity<Guid>, ITenantScoped
{
    // Same fields as NdtInvestmentDecision
    FdiProject Project
}
```

### DRY Consideration

NĐT and FDI entities are nearly identical. However, creating separate classes (not a shared base) follows the TPT pattern established in Phase 1 where DomesticProject and OdaProject are separate despite field overlap. **Rationale:** Keeps EF configuration simple, avoids extra abstraction layer, each type can diverge independently if BA later specifies differences.

### API Endpoints — NĐT

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `api/v1/ndt-projects` | BTC,CQCQ,CDT | List |
| GET | `api/v1/ndt-projects/{id}` | BTC,CQCQ,CDT | Detail |
| POST | `api/v1/ndt-projects` | BTC,CDT | Create |
| PUT | `api/v1/ndt-projects/{id}` | BTC,CDT | Update |
| DELETE | `api/v1/ndt-projects/{id}` | BTC,CDT | Soft delete |
| POST | `api/v1/ndt-projects/{id}/locations` | BTC,CDT | Add location |
| DELETE | `api/v1/ndt-projects/{id}/locations/{lid}` | BTC,CDT | Remove |
| POST | `api/v1/ndt-projects/{id}/decisions` | BTC,CDT | Add decision |
| DELETE | `api/v1/ndt-projects/{id}/decisions/{did}` | BTC,CDT | Remove |
| POST | `api/v1/ndt-projects/{id}/certificates` | BTC,CDT | Add GCNĐKĐT |
| PUT | `api/v1/ndt-projects/{id}/certificates/{cid}` | BTC,CDT | Update |
| DELETE | `api/v1/ndt-projects/{id}/certificates/{cid}` | BTC,CDT | Remove |
| POST | `api/v1/ndt-projects/{id}/bid-packages` | BTC,CDT | Add (reuse) |
| DELETE | `api/v1/ndt-projects/{id}/bid-packages/{bid}` | BTC,CDT | Remove |
| POST | `api/v1/ndt-projects/{id}/documents` | BTC,CDT | Add (reuse) |
| DELETE | `api/v1/ndt-projects/{id}/documents/{docid}` | BTC,CDT | Remove |

**FDI endpoints:** Identical structure, route prefix `api/v1/fdi-projects`.

### FE Component Hierarchy — NĐT

```
web/src/features/ndt-projects/
├── index.ts
├── ndt-project-types.ts
├── ndt-project-api.ts
├── ndt-project-list-page.tsx
├── ndt-project-list-filters.tsx
├── ndt-project-create-page.tsx
├── ndt-project-edit-page.tsx
├── ndt-project-detail-page.tsx
├── ndt-project-tabs-container.tsx         # 5-tab (no HĐ NĐT, no TKTT)
└── tabs/
    ├── ndt-tab1-general-info.tsx           # Adapt from DNNN tab1 (remove TKTT section)
    ├── ndt-tab1-decisions-zone.tsx         # Same DNNN capital structure
    ├── ndt-tab1-locations-zone.tsx         # Same as DNNN (with KKT/KCN)
    ├── ndt-tab1-certificates-zone.tsx      # Reuse DNNN pattern (GCNĐKĐT)
    └── ndt-tab2-implementation.tsx         # THTH (adapt from DNNN tab3)
```

### FE Component Hierarchy — FDI

```
web/src/features/fdi-projects/
├── index.ts
├── fdi-project-types.ts
├── fdi-project-api.ts
├── fdi-project-list-page.tsx
├── fdi-project-list-filters.tsx
├── fdi-project-create-page.tsx
├── fdi-project-edit-page.tsx
├── fdi-project-detail-page.tsx
├── fdi-project-tabs-container.tsx
└── tabs/
    ├── fdi-tab1-general-info.tsx           # Clone from NĐT (label changes only)
    ├── fdi-tab1-decisions-zone.tsx         # Clone from NĐT
    ├── fdi-tab1-locations-zone.tsx         # Clone from NĐT
    ├── fdi-tab1-certificates-zone.tsx      # Clone from NĐT
    └── fdi-tab2-implementation.tsx         # Clone from NĐT
```

### Tab Mapping — NĐT/FDI (identical)

| Tab | Component | Source |
|-----|-----------|--------|
| 1. Thông tin QĐĐT | ndt/fdi-tab1-general-info | DNNN tab1 minus TKTT section |
| 2. Tình hình TH | ndt/fdi-tab2-implementation | DNNN tab3 |
| 3. Thanh tra/KT | shared/tab4-inspection | 100% reuse |
| 4. Khai thác | shared/tab5-operation | 100% reuse |
| 5. Tài liệu | shared/tab6-documents | 100% reuse |

## Related Code Files

### CREATE — Backend (12 files)

| File | Description |
|------|-------------|
| `Domain/Entities/NdtProject.cs` | TPT child |
| `Domain/Entities/FdiProject.cs` | TPT child |
| `Domain/Entities/NdtInvestmentDecision.cs` | NĐT decisions |
| `Domain/Entities/FdiInvestmentDecision.cs` | FDI decisions |
| `Application/Commands/CreateNdtProject/*` | 3 files: command, handler, validator |
| `Application/Commands/UpdateNdtProject/*` | 3 files |
| `Application/Commands/CreateFdiProject/*` | 3 files |
| `Application/Commands/UpdateFdiProject/*` | 3 files |
| `Application/Commands/SubEntities/ManageNdtDecisionCommands.cs` | |
| `Application/Commands/SubEntities/ManageFdiDecisionCommands.cs` | |
| `Application/Queries/ListNdtProjectsQuery.cs` | |
| `Application/Queries/GetNdtProjectByIdQuery.cs` | |
| `Application/Queries/ListFdiProjectsQuery.cs` | |
| `Application/Queries/GetFdiProjectByIdQuery.cs` | |
| `Application/DTOs/NdtProjectDtos.cs` | |
| `Application/DTOs/FdiProjectDtos.cs` | |
| `Infrastructure/Persistence/Configurations/NdtProjectConfiguration.cs` | |
| `Infrastructure/Persistence/Configurations/FdiProjectConfiguration.cs` | |
| `Presentation/Controllers/NdtProjectsController.cs` | |
| `Presentation/Controllers/FdiProjectsController.cs` | |

### CREATE — Frontend (20 files: 10 NĐT + 10 FDI)

Files listed in Architecture section above.

### MODIFY

| File | Change |
|------|--------|
| `Domain/Enums/ProjectType.cs` | Add `Ndt = 5, Fdi = 6` |
| `Infrastructure/Persistence/InvestmentProjectsDbContext.cs` | Add 4 new DbSets |
| App router config | Add NĐT + FDI routes |
| Sidebar/menu config | Add NĐT + FDI menu entries |

## Implementation Steps

### BE — NĐT (1.5d)

1. Add `Ndt = 5, Fdi = 6` to `ProjectType` enum
2. Create `NdtProject` entity — copy DnnnProject, remove DesignEstimate + InvestorSelection navs, set ProjectType=Ndt
3. Create `NdtInvestmentDecision` — copy DnnnInvestmentDecision, change nav to NdtProject
4. Create `NdtProjectConfiguration` — TPT child "NdtProjects", wire RegistrationCertificate (reuse)
5. Create NdtInvestmentDecision EF config
6. Create `CreateNdtProjectCommand` + Handler + Validator — adapt from DNNN
7. Create `UpdateNdtProjectCommand` + Handler + Validator
8. Create `ManageNdtDecisionCommands.cs` — same DNNN capital validation
9. Create `ListNdtProjectsQuery` + `GetNdtProjectByIdQuery`
10. Create `NdtProjectDtos.cs`
11. Create `NdtProjectsController` — same as DNNN minus investor-selection and design-estimate endpoints
12. Register DbSets, generate migration part 1

### BE — FDI (0.5d)

13. Create `FdiProject` entity — copy NdtProject, set ProjectType=Fdi
14. Create `FdiInvestmentDecision` — copy NdtInvestmentDecision
15. Create `FdiProjectConfiguration` + FdiInvestmentDecision EF config
16. Create `CreateFdiProjectCommand` + Handler + Validator — copy from NĐT
17. Create `UpdateFdiProjectCommand` + Handler + Validator
18. Create `ManageFdiDecisionCommands.cs`
19. Create queries + DTOs + controller — copy from NĐT
20. Register DbSets, generate migration: `dotnet ef migrations add AddNdtFdiProjectTypes`

### FE — NĐT (1d)

21. Create `ndt-project-types.ts` — adapt from DNNN (remove investorSelection, designEstimates)
22. Create `ndt-project-api.ts` — adapt from DNNN (remove investor-selection, design-estimate hooks)
23. Create `ndt-tab1-general-info.tsx` — copy DNNN tab1, remove TKTT section
24. Create `ndt-tab1-decisions-zone.tsx` — copy from DNNN (same capital structure)
25. Create `ndt-tab1-locations-zone.tsx` — copy from DNNN (with KKT/KCN)
26. Create `ndt-tab1-certificates-zone.tsx` — copy from DNNN
27. Create `ndt-tab2-implementation.tsx` — adapt from DNNN tab3
28. Create `ndt-project-tabs-container.tsx` — 5-tab container
29. Create list page, filters, create/edit/detail wrappers, index

### FE — FDI (0.5d)

30. Copy NĐT FE folder → `fdi-projects/`
31. Find-replace: `ndt` → `fdi`, `Ndt` → `Fdi`, `NĐT` → `FDI`
32. Adjust labels if any FDI-specific text differences
33. Create FDI routes + menu entry

### Compile & Verify (0.5d)

34. `dotnet build` — verify BE compiles
35. `tsc` / `vite build` — verify FE compiles
36. Apply migration
37. Smoke-test CRUD for both NĐT and FDI

## Todo Checklist

### Backend
- [ ] ProjectType enum: add Ndt=5, Fdi=6
- [ ] NdtProject entity + factory
- [ ] FdiProject entity + factory
- [ ] NdtInvestmentDecision entity
- [ ] FdiInvestmentDecision entity
- [ ] EF configs for all 4 new entities
- [ ] DbContext: register 4 new DbSets
- [ ] EF migration
- [ ] NĐT: command/handler/validator (create + update)
- [ ] FDI: command/handler/validator (create + update)
- [ ] NĐT: decision commands, queries, DTOs
- [ ] FDI: decision commands, queries, DTOs
- [ ] NdtProjectsController
- [ ] FdiProjectsController
- [ ] Compile check

### Frontend
- [ ] NĐT: types, api, all tab components
- [ ] NĐT: tabs-container (5-tab), list, create/edit/detail pages
- [ ] FDI: clone from NĐT, rename
- [ ] FDI: tabs-container, list, create/edit/detail pages
- [ ] Routes + sidebar for both
- [ ] Compile check

## Success Criteria
- `POST /api/v1/ndt-projects` creates NĐT with ProjectType=Ndt
- `POST /api/v1/fdi-projects` creates FDI with ProjectType=Fdi
- NĐT/FDI have: GCNĐKĐT, decisions (CSH/ODA/TCTD), locations (KKT/KCN), bid packages, documents
- NĐT/FDI do NOT have: design estimates, investor selection endpoints
- FE: 5-tab containers render correctly, GCNĐKĐT works, shared tabs render
- Migration creates 4 new tables (NdtProjects, FdiProjects, NdtInvestmentDecisions, FdiInvestmentDecisions)

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| BA later specifies NĐT/FDI field differences | Medium | Low | Entities are separate classes — can diverge without refactoring |
| Too much code duplication (NĐT≈FDI) | Low | Low | Acceptable for TPT pattern; shared entity structure allows future extraction if needed |
| 6 TPT children total — EF query perf | Medium | Medium | Benchmark ListAll query; add ProjectType filter at DB level |
| FDI label changes unclear | Low | Low | Use generic labels; adjust when BA clarifies |

## Security Considerations
- Same auth: BTC/CQCQ/CDT read, BTC/CDT write
- Tenant isolation
- IDOR on sub-entities
