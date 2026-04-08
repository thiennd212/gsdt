# Phase 00 — Prep: data-testid + FE Fixes

**Status:** Complete | **Effort:** 2d | **Tests:** — (enabler phase)
**Red-team:** Addresses F3 (zero data-testid), F4 (filter dropdowns non-functional)

## Overview

Prerequisites before any E2E test can be written. Two workstreams:
1. Add `data-testid` attributes to all interactive elements across active FE modules
2. Fix filter dropdowns in domestic/ODA list pages (currently decorative)

## Workstream 1: data-testid Attributes

### Naming Convention
```
data-testid="{module}-{element}-{name}"

Examples:
  data-testid="domestic-btn-save-tab1"
  data-testid="domestic-input-project-code"
  data-testid="domestic-select-managing-authority"
  data-testid="domestic-table-projects"
  data-testid="catalog-btn-create"
  data-testid="sidebar-menu"
  data-testid="breadcrumb"
```

### Modules to Instrument (priority order)

| Priority | Module | Key Elements | Est. Attrs |
|----------|--------|-------------|------------|
| P0 | domestic-projects | Tabs, forms, table, buttons, filters | ~40 |
| P0 | oda-projects | Same structure as domestic | ~35 |
| P0 | admin-catalogs | Table, modal, buttons | ~15 |
| P0 | shared/components | MoneyInput, DatePicker, FileUpload | ~10 |
| P1 | users | Table, modal, search | ~10 |
| P1 | auth (login flow) | Login button, callback indicators | ~5 |
| P2 | All other modules | Minimal: page container + primary action | ~30 |
| — | sidebar-menu, topbar, breadcrumb | Navigation elements | ~10 |

**Total: ~155 data-testid attributes across 38 modules**

### Rules
- Add to: buttons, inputs, selects, tables, modals, tabs, form items
- Do NOT add to: decorative elements, layout divs, icons
- Shared components (AdminPageHeader, AdminTableToolbar, etc.) get testid via prop passthrough
- Ant Design Form.Item: add to the wrapper, not the inner input (Ant handles forwarding)

## Workstream 2: Fix Filter Dropdowns (F4)

### Problem
`domestic-project-list-filters.tsx` and `oda-project-list-page.tsx` render Select dropdowns for CQ quản lý, Chủ đầu tư, Nhóm DA, Tình trạng — but the selected values are never passed to parent or to API params.

### Fix
1. Extend `DomesticProjectListParams` to include filter fields
2. Wire `onChange` handlers to propagate filter values to parent state
3. Include filter values in API query params
4. Verify BE endpoint accepts these filter params (may need BE changes)

### Files to Modify
- `web/src/features/domestic-projects/domestic-project-types.ts` — add filter fields
- `web/src/features/domestic-projects/domestic-project-list-filters.tsx` — wire onChange
- `web/src/features/domestic-projects/domestic-project-list-page.tsx` — pass filter state
- `web/src/features/oda-projects/oda-project-list-page.tsx` — same pattern

## Todo
- [ ] Add data-testid to domestic-projects (all tabs, forms, table, buttons)
- [ ] Add data-testid to oda-projects
- [ ] Add data-testid to admin-catalogs
- [ ] Add data-testid to shared components (passthrough prop)
- [ ] Add data-testid to sidebar-menu, topbar, breadcrumb
- [ ] Add data-testid to identity/users module
- [ ] Add minimal data-testid to remaining 25+ modules (page container + primary action)
- [ ] Fix domestic filter dropdowns (wire onChange → params → API)
- [ ] Fix ODA filter dropdowns
- [ ] Verify tsc + vite build clean
- [ ] Commit: `feat: add data-testid attributes for E2E + fix filter dropdowns`

## Success Criteria
- `grep -r "data-testid" web/src/features/ | wc -l` >= 100
- All filter dropdowns propagate to API calls (verify in Network tab)
- tsc + vite build pass
