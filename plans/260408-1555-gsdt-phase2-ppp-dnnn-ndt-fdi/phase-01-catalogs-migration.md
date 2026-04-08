# Phase 01 — Catalogs & Migration

## Context Links
- SRS Analysis: `plans/reports/srs-analysis-260408-1527-gsdt-v12-delta.md` (sections 7.1-7.3)
- Existing catalogs: `src/modules/masterdata/GSDT.MasterData/`
- Admin FE: `web/src/features/admin-catalogs/`
- KHLCNT reference: `src/modules/masterdata/GSDT.MasterData/Entities/ContractorSelectionPlan.cs`

## Overview
- **Priority:** P1 (blocks all other phases)
- **Status:** Complete
- **Effort:** 2-3 days (actual)
- **Commit:** 0e6533c
- **Date:** 2026-04-08
- **Description:** Added GovernmentAgency (hierarchical) and Investor (flat) catalogs to MasterData module. Verified KHLCNT columns. Extended Province/Ward with EffectiveDate + Status. Generated EF migration.

## Key Insights

> **IMPORTANT: MasterData Module Clarification**
> The active MasterData module is `GSDT.MasterData` (combined project at `src/modules/masterdata/GSDT.MasterData/`).
> A legacy 4-layer split (`GSDT.MasterData.Domain/Infrastructure/etc`) exists but is NOT the active module.
> All Phase 2 catalog additions (GovernmentAgency, Investor, Province/Ward updates) go into the combined project.
> The 4-layer split project should be deprecated/removed in Phase 8 (buffer).

- GovernmentAgency (T27) is **hierarchical** (self-referencing ParentId) — more complex than flat catalogs
- Investor (T28) is simple 5-field flat catalog — follows ContractorSelectionPlan pattern
- Province/Ward already exist as seed data entities — need 2 additive nullable columns each
- KHLCNT already has NameEn/SignedDate/SignedBy from Phase 1 — **verify only, no changes needed**
- GovernmentAgency replaces "Co quan quan ly" (managing-agencies) in CATALOG_CONFIG for PPP/DNNN — existing TN/ODA continue using managing-authorities catalog

## Requirements

### Functional
1. GovernmentAgency CRUD (hierarchical tree with parent-child)
2. Investor CRUD (flat list with type filter)
3. Province: add EffectiveDate (nullable DateTime), Status (enum: Active/Merged)
4. Ward: add EffectiveDate (nullable DateTime), Status (enum: Active/Merged)
5. KHLCNT: verify existing 3 columns (NameEn, SignedDate, SignedBy) — no action if present

### Non-Functional
- All new entities: tenant-scoped, auditable, soft-delete via IsActive
- GovernmentAgency tree: max 4 levels (Bo/Tinh -> So/Quan -> Phong -> Don vi)
- API response time < 200ms for flat lists, < 500ms for tree queries

## Architecture

### GovernmentAgency Entity (T27 — 13 fields)

```csharp
// src/modules/masterdata/GSDT.MasterData/Entities/GovernmentAgency.cs
public class GovernmentAgency : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId
    string Name               // Required, max 500
    string Code               // Required, unique per tenant, max 50
    Guid? ParentId             // Self-reference (hierarchy)
    string? AgencyType         // "Cac Tinh", "Cac Bo, Ban nganh", "Quan/Huyen", "Tong cong ty"
    string? Origin             // Nguon goc
    string? LdaServer          // LDAServer
    string? Address            // Dia chi
    string? Phone              // Dien thoai, max 20
    string? Fax                // max 20
    string? Email              // max 200
    string? Notes              // Ghi chu
    int SortOrder              // Thu tu
    int? ReportDisplayOrder    // Thu tu hien thi trong BCGSTT
    bool IsActive              // default true
    // Navigation
    GovernmentAgency? Parent
    ICollection<GovernmentAgency> Children
}
```

### Investor Entity (T28 — 5 fields)

```csharp
// src/modules/masterdata/GSDT.MasterData/Entities/Investor.cs
public class Investor : AuditableEntity<Guid>, ITenantScoped
{
    Guid TenantId
    string InvestorType        // "Doanh nghiep", "Ca nhan", "To chuc khac" — max 100
    string BusinessIdOrCccd    // Ma so DN hoac CCCD — max 50, unique per tenant
    string NameVi              // Required, max 500
    string? NameEn             // max 500
    bool IsActive              // default true
}
```

### Province/Ward Extensions

```csharp
// ADD to existing Province entity:
DateTime? EffectiveDate        // Ngay hieu luc
AdministrativeStatus Status    // enum { Active = 1, Merged = 2 } — default Active

// ADD to existing Ward entity (same 2 fields):
DateTime? EffectiveDate
AdministrativeStatus Status
```

### New Enum

```csharp
// src/modules/masterdata/GSDT.MasterData/Enums/AdministrativeStatus.cs
public enum AdministrativeStatus { Active = 1, Merged = 2 }
```

## Related Code Files

### CREATE
| File | Layer |
|------|-------|
| `src/modules/masterdata/GSDT.MasterData/Entities/GovernmentAgency.cs` | Domain |
| `src/modules/masterdata/GSDT.MasterData/Entities/Investor.cs` | Domain |
| `src/modules/masterdata/GSDT.MasterData/Enums/AdministrativeStatus.cs` | Domain |
| `src/modules/masterdata/GSDT.MasterData/Persistence/GovernmentAgencyConfiguration.cs` | Infra |
| `src/modules/masterdata/GSDT.MasterData/Persistence/InvestorConfiguration.cs` | Infra |
| `src/modules/masterdata/GSDT.MasterData/Controllers/GovernmentAgenciesController.cs` | API |
| `src/modules/masterdata/GSDT.MasterData/Controllers/InvestorsController.cs` | API |
| `web/src/features/admin-catalogs/government-agency-catalog-page.tsx` | FE |
| `web/src/features/admin-catalogs/government-agency-form-modal.tsx` | FE |
| `web/src/features/admin-catalogs/government-agency-tree.tsx` | FE (tree component) |
| `web/src/features/admin-catalogs/investor-catalog-page.tsx` | FE |
| `web/src/features/admin-catalogs/investor-form-modal.tsx` | FE |

### MODIFY
| File | Change |
|------|--------|
| `src/modules/masterdata/GSDT.MasterData/Entities/Province.cs` | Add EffectiveDate, Status |
| `src/modules/masterdata/GSDT.MasterData/Entities/Ward.cs` | Add EffectiveDate, Status |
| `src/modules/masterdata/GSDT.MasterData/Persistence/MasterDataDbContext.cs` | Add DbSet<GovernmentAgency>, DbSet<Investor> |
| `web/src/features/admin-catalogs/catalog-config.ts` | Add GovernmentAgency + Investor metadata |
| `web/src/features/admin-catalogs/catalog-api.ts` | Add API hooks for new catalogs |
| `web/src/features/master-data/master-data-page.tsx` | Add nav entries for new catalogs |

## Implementation Steps

### BE (1-1.5d)

1. Create `AdministrativeStatus` enum in `src/modules/masterdata/GSDT.MasterData/Enums/`
2. Create `GovernmentAgency` entity with factory method, self-referencing ParentId
3. Create `Investor` entity with factory method
4. Add `EffectiveDate` (DateTime?) and `Status` (AdministrativeStatus, default Active) to `Province` entity
5. Add same 2 fields to `Ward` entity
6. Create `GovernmentAgencyConfiguration` — unique index on (Code, TenantId), self-referencing FK, max lengths
7. Create `InvestorConfiguration` — unique index on (BusinessIdOrCccd, TenantId)
8. Register DbSets in `MasterDataDbContext`
9. Generate migration: `dotnet ef migrations add AddPhase2Catalogs --project src/modules/masterdata/GSDT.MasterData`
10. Create `GovernmentAgenciesController` — CRUD + tree endpoint (`GET /tree` returns nested JSON)
    - Routes: `api/v1/masterdata/government-agencies`
    - `GET /` — flat list (paginated)
    - `GET /tree` — hierarchical tree (recursive CTE or in-memory build)
    - `GET /{id}` — single item
    - `POST /` — create
    - `PUT /{id}` — update
    - `DELETE /{id}` — soft delete (set IsActive=false)
    - All: `[Authorize]`
11. Create `InvestorsController` — standard CRUD
    - Routes: `api/v1/masterdata/investors`
    - Same CRUD pattern as ContractorSelectionPlansController
    - Filter by InvestorType query param
12. **Data migration:** Migrate existing ManagingAuthority catalog data into GovernmentAgency table. Update TN/ODA `ManagingAuthorityId` references to point to GovernmentAgency records. This ensures all 6 project types use ONE authority source (GovernmentAgency catalog). Run as part of EF migration seed or a standalone data migration script.

### FE (1-1.5d)

12. Add GovernmentAgency + Investor metadata to `catalog-config.ts`
13. Add API hooks to `catalog-api.ts`:
    - `useGovernmentAgencies()`, `useGovernmentAgencyTree()`
    - `useCreateGovernmentAgency()`, `useUpdateGovernmentAgency()`, `useDeleteGovernmentAgency()`
    - `useInvestors()`, `useCreateInvestor()`, `useUpdateInvestor()`, `useDeleteInvestor()`
14. Create `government-agency-tree.tsx` — Ant Design Tree component with drag-drop reorder
15. Create `government-agency-catalog-page.tsx` — tree view + form modal
16. Create `government-agency-form-modal.tsx` — 13-field form with parent selector (TreeSelect)
17. Create `investor-catalog-page.tsx` — flat table (follows KHLCNT pattern)
18. Create `investor-form-modal.tsx` — 5-field form with InvestorType radio group
19. Add navigation entries in `master-data-page.tsx` or sidebar config

## Todo Checklist

- [x] GovernmentAgency entity + EF config
- [x] Investor entity + EF config
- [x] Province: add EffectiveDate + Status
- [x] Ward: add EffectiveDate + Status
- [x] AdministrativeStatus enum
- [x] Register DbSets in MasterDataDbContext
- [x] EF Migration
- [x] GovernmentAgenciesController (CRUD + tree)
- [x] InvestorsController (CRUD)
- [x] FE: government-agency tree page + form
- [x] FE: investor list page + form
- [x] FE: catalog-config + catalog-api updates
- [x] FE: navigation entries
- [x] Verify KHLCNT already has NameEn/SignedDate/SignedBy (no-op if present)
- [x] Compile check (BE + FE)

## Migration Order

> **⚠️ MIGRATION ORDER:** MasterData migrations MUST run before InvestmentProjects migrations. GovernmentAgency and Investor tables must exist before InvestmentProjects module initializes (PppProject/DnnnProject reference GovernmentAgency via CompetentAuthorityId FK).
> Add startup health check: verify GovernmentAgency and Investor tables exist before InvestmentProjects module initializes.

## Success Criteria
- `GET /api/v1/masterdata/government-agencies/tree` returns nested hierarchy
- `POST /api/v1/masterdata/investors` creates investor with unique BusinessIdOrCccd
- Province/Ward entities have new nullable columns without data loss
- Admin UI: GovernmentAgency tree renders with expand/collapse
- Admin UI: Investor CRUD works with type filter
- EF migration applies cleanly on fresh + existing databases

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| GovernmentAgency tree performance with deep hierarchy | Low | Medium | Limit to 4 levels, eager-load children 2 levels deep |
| Province/Ward migration breaks seed data | Low | High | Nullable columns, default Status=Active |
| ParentId self-reference cycle | Low | Medium | Validate in controller: cannot set parent to self or descendant |

## Security Considerations
- All endpoints: `[Authorize]` (admin-level access)
- Tenant isolation: filter by TenantId from JWT
- GovernmentAgency Code + Investor BusinessIdOrCccd: unique per tenant constraint prevents duplicates
