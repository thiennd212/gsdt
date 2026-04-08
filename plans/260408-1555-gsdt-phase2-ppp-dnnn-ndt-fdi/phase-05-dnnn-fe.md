# Phase 05 — DNNN Frontend

## Context Links
- SRS Analysis: `plans/reports/srs-analysis-260408-1527-gsdt-v12-delta.md` (section 9, T24-T25)
- PPP FE reference: `web/src/features/ppp-projects/` (after Phase 3)
- Shared components: `web/src/features/shared/`
- DNNN BE API: Phase 04

## Overview
- **Priority:** P1
- **Status:** Complete
- **Effort:** 4-5 days
- **Blocker:** Phase 4 (DNNN API must be operational)
- **Description:** Build DNNN project frontend — 6-tab form, list page. Tab1 complex (GCNĐKĐT section, KKT/KCN location field, DNNN capital structure). Tab2 (HĐ NĐT) reuses InvestorSelection from PPP. Tabs 3-6 share/adapt from PPP + shared components. ~55% reuse from PPP FE.

## Key Insights
- DNNN Tab1 has 84 fields (most of any type) but many sections overlap with PPP Tab1
- GCNĐKĐT is a new inline CRUD section (list with Sửa/Xóa) — unique to DNNN/NĐT/FDI
- KKT/KCN field: extends location row with extra text field + tooltip
- Tab2 (HĐ NĐT): simpler than PPP Tab2 — just InvestorSelection section, no TMĐT/HĐ ký kết
- THTH tab: similar to PPP Tab3 but with DNNN capital labels (no capital plan section)
- Thanh tra, Khai thác, Tài liệu: reuse shared tabs directly
- List page: 5 filters (simpler than PPP's 8)

## Requirements

### Functional
1. DNNN project list page with 5 filters (tên, CQCQ, NĐT, tình trạng, địa điểm)
2. Create/Edit/Detail pages with 6-tab container
3. Tab1: General info with DNNN capital (CSH/ODA/TCTD), GCNĐKĐT section, KKT/KCN location, TKTT popup
4. Tab2: Investor selection (reuse PPP component) — simpler, no contract info
5. Tab3: THTH (bid packages, execution records — no capital plan section)
6. Tab4: Inspection (reuse shared)
7. Tab5: Operation (reuse shared — no revenue reporting)
8. Tab6: Documents (reuse shared)

### Non-Functional
- Same patterns as PPP FE: Vietnamese labels, 2-column layout, money-input, file upload validation
- Maximize component reuse from PPP and shared

## Architecture

### Component Hierarchy

```
web/src/features/dnnn-projects/
├── index.ts
├── dnnn-project-types.ts
├── dnnn-project-api.ts
├── dnnn-project-list-page.tsx
├── dnnn-project-list-filters.tsx
├── dnnn-project-create-page.tsx
├── dnnn-project-edit-page.tsx
├── dnnn-project-detail-page.tsx
├── dnnn-project-tabs-container.tsx
└── tabs/
    ├── dnnn-tab1-general-info.tsx          # Main form (DNNN capital, GCNĐKĐT, TKTT)
    ├── dnnn-tab1-decisions-zone.tsx        # QĐ CTĐT + QĐ ĐT (DNNN capital: CSH/ODA/TCTD)
    ├── dnnn-tab1-locations-zone.tsx        # Locations + KKT/KCN field
    ├── dnnn-tab1-certificates-zone.tsx     # GCNĐKĐT inline CRUD (NEW)
    ├── dnnn-tab2-investor-contract.tsx     # HĐ NĐT (reuse InvestorSelection from PPP)
    └── dnnn-tab3-implementation.tsx        # THTH (bid packages + execution)

# Reuse from shared (no new files needed):
# Tab4 → shared/tabs/tab4-inspection.tsx
# Tab5 → shared/tabs/tab5-operation.tsx (refactored in Phase 3 to accept configurable hook)
# Tab6 → shared/tabs/tab6-documents.tsx

# Reuse from PPP (import, not copy):
# ppp-tab2-investor-selection.tsx → import and wrap in dnnn-tab2
# design-estimate-popup.tsx → already shared
```

### Tab Mapping

| DNNN Tab | Component | Reuse Source | Notes |
|----------|-----------|-------------|-------|
| 1. Thông tin QĐĐT | dnnn-tab1-general-info + sub-components | ~50% from PPP tab1 | New: GCNĐKĐT, KKT/KCN, DNNN capital |
| 2. HĐ NĐT | dnnn-tab2-investor-contract | ~80% from PPP tab2 investor-selection | No TMĐT/contract sections |
| 3. Tình hình TH | dnnn-tab3-implementation | ~70% from PPP tab3 | No capital plan section |
| 4. Thanh tra/KT | shared/tab4-inspection | 100% | Direct reuse |
| 5. Khai thác | shared/tab5-operation | 100% | No revenue (PPP-only) |
| 6. Tài liệu | shared/tab6-documents | 100% | Direct reuse |

### Key DNNN-specific FE Logic

1. **DNNN Capital Breakdown** (decisions-zone):
   - TMĐT = Vốn CSH + Vốn vay ODA + Vốn vay TCTD (enforced on save)
   - Tỷ lệ CSH = Vốn CSH / TMĐT (auto-calc, 2 decimals)
   - Different labels from PPP (no NSTW/NSĐP/NSNN khác)

2. **GCNĐKĐT Section** (certificates-zone):
   - Inline form: Số GCN, Ngày cấp, File, Vốn ĐT, Vốn CSH, Tỷ lệ CSH (auto-calc)
   - Insert → shows in table below with Sửa/Xóa actions
   - Multiple records allowed per project

3. **KKT/KCN Location Field** (locations-zone):
   - Extends standard location row with: "Tên KKT, KCN, KCX, FTZ, TTTC" text field
   - Has tooltip explaining the field
   - Maps to ProjectLocation.IndustrialZoneName

## Related Code Files

### CREATE (14 files)

| File | Description |
|------|-------------|
| `web/src/features/dnnn-projects/index.ts` | Barrel exports |
| `web/src/features/dnnn-projects/dnnn-project-types.ts` | TS interfaces |
| `web/src/features/dnnn-projects/dnnn-project-api.ts` | React Query hooks |
| `web/src/features/dnnn-projects/dnnn-project-list-page.tsx` | List page |
| `web/src/features/dnnn-projects/dnnn-project-list-filters.tsx` | 5-filter bar |
| `web/src/features/dnnn-projects/dnnn-project-create-page.tsx` | Create page |
| `web/src/features/dnnn-projects/dnnn-project-edit-page.tsx` | Edit page |
| `web/src/features/dnnn-projects/dnnn-project-detail-page.tsx` | Detail page |
| `web/src/features/dnnn-projects/dnnn-project-tabs-container.tsx` | 6-tab container |
| `web/src/features/dnnn-projects/tabs/dnnn-tab1-general-info.tsx` | Tab1 main form |
| `web/src/features/dnnn-projects/tabs/dnnn-tab1-decisions-zone.tsx` | QĐ ĐT (DNNN capital) |
| `web/src/features/dnnn-projects/tabs/dnnn-tab1-locations-zone.tsx` | Locations + KKT/KCN |
| `web/src/features/dnnn-projects/tabs/dnnn-tab1-certificates-zone.tsx` | GCNĐKĐT CRUD |
| `web/src/features/dnnn-projects/tabs/dnnn-tab2-investor-contract.tsx` | HĐ NĐT wrapper |
| `web/src/features/dnnn-projects/tabs/dnnn-tab3-implementation.tsx` | THTH |

### MODIFY

| File | Change |
|------|--------|
| App router config | Add DNNN routes: `/dnnn-projects`, `/dnnn-projects/create`, etc. |
| Sidebar/menu config | Add "Dự án DNNN" menu entry |

## Implementation Steps

### Types & API (0.5d)

1. Create `dnnn-project-types.ts`:
   - `DnnnProjectListItem` — id, projectCode, projectName, investorName, competentAuthority, status, location
   - `DnnnProjectDetail` — full detail including certificates[], investorSelection, designEstimates[], bidPackages[], executionRecords[], inspectionRecords[], etc.
   - `CreateDnnnProjectRequest`, `UpdateDnnnProjectRequest`
   - `RegistrationCertificateRequest` type

2. Create `dnnn-project-api.ts`:
   - Query keys: `dnnn-projects` namespace
   - `useDnnnProjects(params)`, `useDnnnProject(id)`
   - `useCreateDnnnProject()`, `useUpdateDnnnProject()`, `useDeleteDnnnProject()`
   - Sub-entity mutations: decisions, certificates (add/update/delete), investor-selection (upsert), design-estimates, bid-packages, documents

### GCNĐKĐT Component (1d)

3. Create `dnnn-tab1-certificates-zone.tsx`:
   - Inline form at top: Số GCN (text), Ngày cấp (DatePicker), File (upload), Vốn ĐT (MoneyInput), Vốn CSH (MoneyInput), Tỷ lệ CSH (auto-calc, disabled)
   - "Thêm" button inserts record via `useAddCertificate()`
   - Table below: columns = Số GCN, Ngày cấp, Vốn ĐT, Vốn CSH, Tỷ lệ, Thao tác (Sửa/Xóa)
   - Edit: opens inline form pre-filled, uses `useUpdateCertificate()`
   - Delete: Popconfirm → `useDeleteCertificate()`

### Tab1 (1d)

4. Create `dnnn-tab1-decisions-zone.tsx`:
   - Adapt from PPP decisions zone — change capital fields to CSH/ODA/TCTD
   - QĐ CTĐT: Số/Ngày/CQ, TMĐT = CSH + ODA + TCTD, File
   - QĐ ĐT: same breakdown + Người ký + Tỷ lệ CSH auto-calc
   - Validation: Total == CSH + ODA + TCTD

5. Create `dnnn-tab1-locations-zone.tsx`:
   - Extend from domestic/PPP locations zone
   - Add "KKT/KCN/KCX/FTZ/TTTC" text field per location row
   - Tooltip: "Tên khu kinh tế, khu công nghiệp, khu chế xuất, FTZ, trung tâm tài chính"
   - Maps to `industrialZoneName` in API payload

6. Create `dnnn-tab1-general-info.tsx`:
   - Section 1: Tên dự án, **NĐT** (textbox), **Tỷ lệ vốn NN** (% input)
   - Section 2: QĐ CTĐT (DecisionsZone with DNNN capital)
   - Section 3: QĐ ĐT (DecisionsZone)
   - Section 4: **GCNĐKĐT** (CertificatesZone)
   - Section 5: Project fields — Mã, CQCQ, Nhóm, Tình trạng, DA thành phần
   - Section 6: Quy mô — Diện tích, Công suất, Hạng mục, Mục tiêu, Tiến độ, Thời gian
   - Section 7: Địa điểm (LocationsZone with KKT/KCN)
   - Section 8: TKTT (DesignEstimatePopup — reuse shared)

### Tab2-3 (1d)

7. Create `dnnn-tab2-investor-contract.tsx`:
   - Import and reuse `InvestorSelection` section from PPP (or extract to shared if not already)
   - Simpler than PPP Tab2: only investor selection section, no TMĐT breakdown, no HĐ ký kết
   - Uses `useUpsertInvestorSelection()` from dnnn-project-api

8. Create `dnnn-tab3-implementation.tsx`:
   - Adapt from PPP tab3-implementation
   - **Remove** capital plan section (DNNN has no "KH bố trí vốn")
   - Keep: bid packages (reuse popup), contracts (reuse popup), execution records table

### Container & Pages (0.5d)

9. Create `dnnn-project-tabs-container.tsx` — 6-tab container:
   - Tab1: Thông tin QĐĐT (always active)
   - Tab2: HĐ NĐT (disabled until projectId)
   - Tab3: Tình hình TH (disabled until projectId)
   - Tab4: Thanh tra/KT → shared/tab4-inspection
   - Tab5: Khai thác → shared/tab5-operation (no revenue)
   - Tab6: Tài liệu → shared/tab6-documents

10. Create list page + filters + create/edit/detail wrappers + index.ts

### List Page (0.5d)

11. Create `dnnn-project-list-page.tsx`:
    - Table columns: STT, Mã DA, Tên DA, NĐT, CQCQ, Địa điểm, Tình trạng, Thao tác

12. Create `dnnn-project-list-filters.tsx`:
    - 5 filters: Tên DA (text), CQCQ (GovernmentAgency select), NĐT (Investor select), Tình trạng (status select), Địa điểm (CascadingLocationSelect)

### Routing (0.5d)

13. Add DNNN routes to app router
14. Add "Dự án DNNN" to sidebar menu

## Todo Checklist

- [x] dnnn-project-types.ts
- [x] dnnn-project-api.ts
- [x] dnnn-tab1-general-info.tsx
- [x] dnnn-tab1-decisions-zone.tsx (DNNN capital: CSH/ODA/TCTD)
- [x] dnnn-tab1-locations-zone.tsx (KKT/KCN field)
- [x] dnnn-tab1-certificates-zone.tsx (GCNĐKĐT inline CRUD)
- [x] dnnn-tab2-investor-contract.tsx (reuse InvestorSelection)
- [x] dnnn-tab3-implementation.tsx (no capital plans)
- [x] dnnn-project-tabs-container.tsx (6-tab)
- [x] dnnn-project-list-page.tsx + list-filters.tsx
- [x] Create/Edit/Detail page wrappers
- [x] index.ts barrel
- [x] Router config: add DNNN routes
- [x] Sidebar: add DNNN menu entry
- [x] Compile check

## Success Criteria
- DNNN project list displays with 5 filter options
- Create flow: Tab1 creates → Tabs 2-6 activate
- GCNĐKĐT: add/edit/delete certificate records inline
- DNNN capital validation: Total == CSH + ODA + TCTD
- KKT/KCN field shows in location rows with tooltip
- TKTT popup works for DNNN (reuses shared component)
- InvestorSelection works (reused from PPP pattern)
- Shared tabs (inspection, operation, documents) render correctly

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| InvestorSelection component from PPP may be tightly coupled | Medium | Medium | Extract to shared if needed during Phase 3 |
| GCNĐKĐT inline CRUD is new pattern (not in PPP) | Low | Medium | Follow bid-package-form-modal pattern |
| KKT/KCN field extends location row — may need CascadingLocationSelect modification | Low | Low | Add optional extra field prop to existing component |

## Security Considerations
- Same auth as PPP: JWT-authenticated apiClient
- Investor select: only show active investors
- Certificate file uploads: validate .pdf
