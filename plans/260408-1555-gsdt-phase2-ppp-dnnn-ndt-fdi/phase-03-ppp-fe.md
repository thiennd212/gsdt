# Phase 03 — PPP Frontend

## Context Links
- SRS Analysis: `plans/reports/srs-analysis-260408-1527-gsdt-v12-delta.md` (section 8, tabs 1-7)
- Domestic FE reference: `web/src/features/domestic-projects/`
- Shared tabs: `web/src/features/shared/tabs/`
- Shared components: `web/src/features/shared/components/`
- Tabs container pattern: `web/src/features/domestic-projects/domestic-project-tabs-container.tsx`
- API hook pattern: `web/src/features/domestic-projects/domestic-project-api.ts`

## Overview
- **Priority:** P1
- **Status:** Complete
- **Effort:** 6-8 days (actual)
- **Commit:** 4fa1612
- **Date:** 2026-04-08
- **Blocker:** Phase 2 (PPP API must be operational)
- **Description:** Built PPP project frontend — 22 React components, 7-tab form (1=cascading contract type, 2=new investor/contract, 6=revenue), list page with filters, API hooks, shared DesignEstimate component. 100% TodoChecklist completion.

## Key Insights
- Tab1 (Thông tin QĐĐT): 71 fields across 7 sections. Has unique Loại HĐ cascading logic
- Tab2 (HĐ dự án PPP): Entirely new — no P1 equivalent. 4 sections, 30 fields
- Tab3 (THTH): Similar to TN Tab2 but with "KH bố trí vốn NN" variant + TKTT popup
- Tab4 (Giải ngân): 3-source disbursement (VonNN/CSH/Vay) vs TN's 2-source
- Tab5 (Thanh tra): Reuse shared tab4-inspection.tsx
- Tab6 (Khai thác+Revenue): Extended operation + PPP-only revenue reporting table
- Tab7 (Tài liệu): Reuse shared tab6-documents.tsx
- TKTT popup: New shared component (DesignEstimate) used by PPP Tab1 + DNNN Tab1

## Requirements

### Functional
1. PPP project list page with 8 search filters (tên, CQCQ, địa điểm, lĩnh vực, loại HĐ, nhóm, tình trạng, tìm kiếm)
2. Create/Edit/Detail pages with 7-tab container
3. Tab1: General info form with cascading contract type, TKTT popup, location multi-row, decisions list
4. Tab2: Investor selection (multiselect from catalog) + contract info form + TMĐT validation
5. Tab3: Capital plans table + bid packages popup (reuse) + contracts popup (reuse) + execution records
6. Tab4: 3-source disbursement form
7. Tab5: Inspection/evaluation/audit (reuse shared)
8. Tab6: Operation checkboxes + revenue reporting CRUD table
9. Tab7: Documents (reuse shared)

### Non-Functional
- All forms: Vietnamese labels, responsive 2-column layout
- Money inputs: use shared MoneyInput component, locale vi-VN
- File uploads: validate >= 1 .pdf
- Tab independence: each tab saves independently (multi-tab save strategy)

## Architecture

### Component Hierarchy

```
web/src/features/ppp-projects/
├── index.ts                           # barrel exports
├── ppp-project-types.ts               # TS types/interfaces
├── ppp-project-api.ts                 # React Query hooks
├── ppp-project-list-page.tsx          # List with filters
├── ppp-project-list-filters.tsx       # Filter bar component
├── ppp-project-create-page.tsx        # Create wrapper
├── ppp-project-edit-page.tsx          # Edit wrapper
├── ppp-project-detail-page.tsx        # Read-only wrapper
├── ppp-project-tabs-container.tsx     # 7-tab container
└── tabs/
    ├── ppp-tab1-general-info.tsx      # Thông tin QĐĐT (main form)
    ├── ppp-tab1-decisions-zone.tsx    # QĐ CTĐT + QĐ ĐT section
    ├── ppp-tab1-locations-zone.tsx    # Địa điểm multi-row (reuse pattern)
    ├── ppp-tab1-contract-type-select.tsx  # Cascading radio+combo for Loại HĐ
    ├── ppp-tab2-contract-details.tsx  # HĐ dự án PPP (entirely new)
    ├── ppp-tab2-investor-selection.tsx # NĐT multiselect section
    ├── ppp-tab2-tmdt-breakdown.tsx    # TMĐT capital validation form
    ├── ppp-tab3-implementation.tsx    # THTH (capital plans + bid packages + execution)
    ├── ppp-tab4-disbursement.tsx      # 3-source disbursement
    └── ppp-tab6-operation-revenue.tsx # Operation + revenue table

web/src/features/shared/
├── components/
│   └── design-estimate-popup.tsx      # NEW: TKTT popup (shared PPP+DNNN)
│   └── design-estimate-items-table.tsx # NEW: Hạng mục detail table
└── tabs/
    (existing tab4-inspection.tsx, tab5-operation.tsx, tab6-documents.tsx — reused as-is for Tab5, Tab7)
```

### Tab Mapping

| PPP Tab | Component | Reuse | Notes |
|---------|-----------|-------|-------|
| 1. Thông tin QĐĐT | ppp-tab1-general-info + sub-components | ~40% from TN tab1 | New: contract type cascading, TKTT popup |
| 2. HĐ dự án PPP | ppp-tab2-contract-details + sub-components | 0% (new) | Investor multiselect, TMĐT, HĐ ký kết |
| 3. Tình hình TH | ppp-tab3-implementation | ~60% from TN tab2 | Different capital plan labels, same bid packages |
| 4. Giải ngân | ppp-tab4-disbursement | ~50% from TN tab3 | 3-source vs 2-source |
| 5. Thanh tra/KT | shared/tab4-inspection | 100% | Direct reuse |
| 6. Khai thác+Revenue | ppp-tab6-operation-revenue | ~30% from shared tab5 | New revenue reporting section |
| 7. Tài liệu | shared/tab6-documents | 100% | Direct reuse |

### Key FE Business Logic

1. **Contract Type Cascading** (ppp-tab1-contract-type-select.tsx):
   - Radio group: BOT / BT / Các loại khác
   - BOT selected → no sub-combo
   - BT selected → sub-combo: Đất / Tiền / Không TT
   - Khác selected → sub-combo: BTO / BOO / O&M / BTL / BLT / HĐ hỗn hợp
   - Final value maps to PppContractType enum (1-10)

2. **TMĐT Validation** (ppp-tab2-tmdt-breakdown.tsx):
   - Tổng TMĐT = Vốn NN + CSH + Vay (enforced on save)
   - Tổng vốn NN = NSTW + NSĐP + NSNN khác (auto-calc, disabled field)
   - Tỷ lệ CSH = Vốn CSH / TMĐT (auto-calc, 2 decimals)

3. **Revenue Cumulative** (ppp-tab6-operation-revenue.tsx):
   - Lũy kế = auto-sum of all RevenueReport.RevenuePeriod for selected year range
   - Displayed but not editable

## Related Code Files

### CREATE (17 files)

| File | Description |
|------|-------------|
| `web/src/features/ppp-projects/index.ts` | Barrel exports |
| `web/src/features/ppp-projects/ppp-project-types.ts` | TS interfaces |
| `web/src/features/ppp-projects/ppp-project-api.ts` | React Query hooks |
| `web/src/features/ppp-projects/ppp-project-list-page.tsx` | List page |
| `web/src/features/ppp-projects/ppp-project-list-filters.tsx` | Filter bar |
| `web/src/features/ppp-projects/ppp-project-create-page.tsx` | Create page |
| `web/src/features/ppp-projects/ppp-project-edit-page.tsx` | Edit page |
| `web/src/features/ppp-projects/ppp-project-detail-page.tsx` | Detail page |
| `web/src/features/ppp-projects/ppp-project-tabs-container.tsx` | 7-tab container |
| `web/src/features/ppp-projects/tabs/ppp-tab1-general-info.tsx` | Tab1 main |
| `web/src/features/ppp-projects/tabs/ppp-tab1-decisions-zone.tsx` | Tab1 decisions |
| `web/src/features/ppp-projects/tabs/ppp-tab1-locations-zone.tsx` | Tab1 locations |
| `web/src/features/ppp-projects/tabs/ppp-tab1-contract-type-select.tsx` | Cascading select |
| `web/src/features/ppp-projects/tabs/ppp-tab2-contract-details.tsx` | Tab2 main |
| `web/src/features/ppp-projects/tabs/ppp-tab2-investor-selection.tsx` | Investor multiselect |
| `web/src/features/ppp-projects/tabs/ppp-tab2-tmdt-breakdown.tsx` | TMĐT form |
| `web/src/features/ppp-projects/tabs/ppp-tab3-implementation.tsx` | Tab3 THTH |
| `web/src/features/ppp-projects/tabs/ppp-tab4-disbursement.tsx` | Tab4 3-source |
| `web/src/features/ppp-projects/tabs/ppp-tab6-operation-revenue.tsx` | Tab6 operation+revenue |
| `web/src/features/shared/components/design-estimate-popup.tsx` | TKTT modal (shared) |
| `web/src/features/shared/components/design-estimate-items-table.tsx` | TKTT items (shared) |

### MODIFY

| File | Change |
|------|--------|
| `web/src/features/shared/components/index.ts` | Export design-estimate components |
| `web/src/features/shared/tabs/tab5-operation.tsx` | Make data source configurable (not hardcoded to useDomesticProject) |
| App router config (varies) | Add PPP routes: `/ppp-projects`, `/ppp-projects/create`, `/ppp-projects/:id`, `/ppp-projects/:id/edit` |
| Sidebar/menu config | Add "Dự án PPP" menu entry under investment projects |

## Implementation Steps

### Types & API (1d)

1. Create `ppp-project-types.ts`:
   - `PppProjectListItem` — id, projectCode, projectName, contractType, competentAuthority, status, location, createdDate
   - `PppProjectDetail` — full detail including all sub-entities (decisions[], investorSelection, contractInfo, capitalPlans[], bidPackages[], designEstimates[], executionRecords[], disbursements[], revenueReports[], inspectionRecords[], evaluationRecords[], auditRecords[], violationRecords[], operation, documents[])
   - `CreatePppProjectRequest`, `UpdatePppProjectRequest`
   - Sub-entity request types: `UpsertInvestorSelectionRequest`, `UpsertPppContractInfoRequest`, etc.
   - `PppContractType` enum (matching BE values)

2. Create `ppp-project-api.ts`:
   - Query keys: `ppp-projects` namespace
   - `usePppProjects(params)` — list
   - `usePppProject(id)` — detail
   - `useCreatePppProject()`, `useUpdatePppProject()`, `useDeletePppProject()`
   - Sub-entity mutations: `useAddPppDecision()`, `useDeletePppDecision()`, `useUpsertInvestorSelection()`, `useUpsertContractInfo()`, `useAddCapitalPlan()`, `useDeleteCapitalPlan()`, `useAddBidPackage()` (reuse from shared), `useAddDesignEstimate()`, `useUpdateDesignEstimate()`, `useDeleteDesignEstimate()`, `useAddDisbursement()`, `useDeleteDisbursement()`, `useAddRevenueReport()`, `useUpdateRevenueReport()`, `useDeleteRevenueReport()`, `useAddDocument()`, `useDeleteDocument()`

### Shared Components (0.5d)

3. Create `design-estimate-popup.tsx` — Ant Design Modal with:
   - QĐ phê duyệt section (number, date, authority, file upload)
   - 7 cost item inputs (MoneyInput) with auto-sum TotalEstimate displayed for UX preview
   - Hạng mục detail table (editable rows via DesignEstimateItemsTable)
   - Props: `projectId`, `mode`, `onSaved`, `editingEstimate?`
   - **⚠️ FE auto-sum is UX preview only. Do NOT send TotalEstimate to API — server recomputes the authoritative value.**

4. Create `design-estimate-items-table.tsx` — inline editable table:
   - Columns: ItemName (text), EstimatedAmount (money), Notes (text), Actions
   - Add row / delete row inline

5. Refactor `tab5-operation.tsx` — accept `projectQueryHook` prop instead of hardcoded `useDomesticProject`, so PPP/DNNN/NĐT/FDI can pass their own query hook

### List Page (1d)

6. Create `ppp-project-list-page.tsx` following domestic-project-list-page pattern:
   - Table columns: STT, Mã DA, Tên DA, Loại HĐ, CQCQ, Địa điểm, Tình trạng, Thao tác (Xem/Sửa/Xóa)
   - Pagination, sorting, search

7. Create `ppp-project-list-filters.tsx`:
   - Tên dự án (text), CQCQ (GovernmentAgency select), Địa điểm (CascadingLocationSelect), Lĩnh vực đầu tư (seed catalog select), Loại HĐ (PppContractType select), Nhóm DA (seed catalog), Tình trạng (status select), Tìm kiếm (global text)

### Tab1 — General Info (1.5d)

8. Create `ppp-tab1-contract-type-select.tsx`:
   - Radio.Group: BOT / BT / Khác
   - Conditional Select below radio based on selection
   - Value output: single PppContractType enum value

9. Create `ppp-tab1-decisions-zone.tsx` following domestic `tab1-decisions-zone.tsx` pattern:
   - QĐ CTĐT section: Có điều chỉnh checkbox, Dừng checkbox, Số/Ngày/CQ QĐ, SBTMLT (PPP capital: VonNN/CSH/Vay), File, Insert to list
   - QĐ ĐT section: same PPP capital breakdown, + Người ký, Tỷ lệ CSH auto-calc

10. Create `ppp-tab1-locations-zone.tsx` following domestic pattern — multi-row Province/Ward

11. Create `ppp-tab1-general-info.tsx` — main Tab1 form:
    - Section 1: Tên dự án
    - Section 2: QĐ CTĐT (DecisionsZone)
    - Section 3: QĐ ĐT (DecisionsZone)
    - Section 4: Project fields — Mã, Lĩnh vực, Tình trạng, Nhóm, **Loại HĐ** (ContractTypeSelect), DA thành phần, CQCQ, Đơn vị chuẩn bị, Mục tiêu
    - Section 5: Quy mô — Diện tích, Công suất, Hạng mục chính
    - Section 6: Địa điểm (LocationsZone)
    - Section 7: TKTT (DesignEstimatePopup trigger button + list)

### Tab2 — Contract Details (1.5d)

12. Create `ppp-tab2-investor-selection.tsx`:
    - Hình thức lựa chọn: Select (4 options)
    - NĐT: Ant Design Select mode="multiple" with options from `useInvestors()` catalog hook
    - QĐ lựa chọn: number, date, file
    - Save via `useUpsertInvestorSelection()`

13. Create `ppp-tab2-tmdt-breakdown.tsx`:
    - TMĐT total (auto-calc = VonNN + CSH + Vay)
    - Vốn NN total (auto-calc = NSTW + NSĐP + NSNN khác) — disabled
    - NSTW, NSĐP, NSNN khác inputs
    - Vốn CSH input + Tỷ lệ CSH (auto-calc, disabled)
    - Vốn vay input
    - All MoneyInput components
    - Form.Item validation: total must match

14. Create `ppp-tab2-contract-details.tsx` — main Tab2 form:
    - Section 1: InvestorSelection sub-component
    - Section 2: TmdtBreakdown sub-component
    - Section 3: Tiến độ — progress text, contract duration, revenue sharing mechanism
    - Section 4: HĐ ký kết — authority, number, date, construction start, completion date

### Tab3-4 (1d)

15. Create `ppp-tab3-implementation.tsx`:
    - Capital plans table (AllocationRound, StateCapitalByDecision) with add/delete
    - Bid packages section (reuse bid-package-form-modal from domestic)
    - Contracts section (reuse)
    - Execution records table

16. Create `ppp-tab4-disbursement.tsx`:
    - 3-column layout: Vốn NN (period + cumulative), Vốn CSH (period + cumulative), Vốn vay (period + cumulative)
    - Report date picker
    - Add/delete rows

### Tab6 (0.5d)

17. Create `ppp-tab6-operation-revenue.tsx`:
    - Settlement checkboxes: QT vốn ĐTC, Xác nhận hoàn thành, QT vốn XD, Đưa vào khai thác
    - Revenue reporting table: Năm (select), Kỳ (select: 6m/1y), Doanh thu kỳ (money), Lũy kế (auto-calc, disabled), Chia sẻ tăng/giảm (text), Khó khăn, Kiến nghị
    - CRUD rows via useAddRevenueReport / useUpdateRevenueReport / useDeleteRevenueReport

### Container & Pages (0.5d)

18. Create `ppp-project-tabs-container.tsx` — 7-tab container following domestic pattern:
    - Tab1: always active
    - Tabs 2-7: disabled until projectId exists
    - Tab5 → shared tab4-inspection, Tab7 → shared tab6-documents

19. Create `ppp-project-create-page.tsx`, `ppp-project-edit-page.tsx`, `ppp-project-detail-page.tsx` — wrapper pages following domestic pattern

20. Create `index.ts` barrel exports

### Routing & Navigation (0.5d)

21. Add PPP routes to app router:
    - `/ppp-projects` → PppProjectListPage
    - `/ppp-projects/create` → PppProjectCreatePage
    - `/ppp-projects/:id` → PppProjectDetailPage
    - `/ppp-projects/:id/edit` → PppProjectEditPage

22. Add "Dự án PPP" to sidebar menu under investment projects section

## Todo Checklist

- [x] ppp-project-types.ts (all TS interfaces)
- [x] ppp-project-api.ts (all query/mutation hooks)
- [x] Shared: design-estimate-popup.tsx
- [x] Shared: design-estimate-items-table.tsx
- [x] Refactor tab5-operation.tsx (configurable data source)
- [x] ppp-project-list-page.tsx + list-filters.tsx
- [x] ppp-tab1-general-info.tsx (main form)
- [x] ppp-tab1-contract-type-select.tsx (cascading)
- [x] ppp-tab1-decisions-zone.tsx
- [x] ppp-tab1-locations-zone.tsx
- [x] ppp-tab2-contract-details.tsx (main)
- [x] ppp-tab2-investor-selection.tsx
- [x] ppp-tab2-tmdt-breakdown.tsx (TMĐT validation)
- [x] ppp-tab3-implementation.tsx
- [x] ppp-tab4-disbursement.tsx (3-source)
- [x] ppp-tab6-operation-revenue.tsx (revenue table)
- [x] ppp-project-tabs-container.tsx (7-tab)
- [x] Create/Edit/Detail page wrappers
- [x] index.ts barrel
- [x] Router config: add PPP routes
- [x] Sidebar: add PPP menu entry
- [x] Compile check (tsc, vite build)

## Success Criteria
- PPP project list displays with all 8 filter options working
- Create flow: Tab1 creates project → Tab2-7 activate → each tab saves independently
- Contract type cascading: selecting BT shows sub-combo, selecting Khác shows different sub-combo
- TMĐT validation prevents save when total mismatch
- TKTT popup creates design estimate with 7 cost items + hạng mục detail
- Revenue table: add/edit/delete rows, cumulative auto-calculates
- Shared tabs (inspection, documents) render correctly for PPP projects
- All pages responsive, Vietnamese labels, no console errors

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Tab2 entirely new — no reference pattern | Medium | High | Build investor-selection + tmdt-breakdown as isolated sub-components, test independently |
| Contract type cascading logic complex | Medium | Medium | Isolate in ppp-tab1-contract-type-select.tsx, unit test all 10 enum mappings |
| tab5-operation.tsx refactor may break TN/ODA | Low | High | Add backward-compatible prop default; test TN/ODA still render |
| 21 files to create — risk of inconsistent patterns | Low | Medium | Follow domestic-projects folder structure exactly, copy-adapt pattern |
| Revenue cumulative auto-calc may have rounding issues | Low | Low | Use same decimal handling as money-input, round to 4 decimals |

## Security Considerations
- API calls go through authenticated apiClient (JWT token in header)
- File uploads validate .pdf extension on client + server
- Investor multiselect: only show IsActive investors from catalog
