# Phase 07 — Cross-Cutting, Smoke, a11y, Performance E2E

**Status:** Pending | **Effort:** 1.5d | **Tests:** 14 | **Depends on:** P01
**Red-team:** Addresses F14 (a11y), F15 (perf), F17 (i18n ordering)

## Navigation Smoke (3 tests)
| # | Test | Expected |
|---|------|----------|
| 1 | `should access all sidebar links without error` | No 404/500 on any link |
| 2 | `should show breadcrumb on project pages` | Breadcrumb with correct path |
| 3 | `should show 404 for invalid route` | /nonexistent → Not Found page |

## Responsive (2 tests)
| # | Test | Expected |
|---|------|----------|
| 4 | `should collapse sidebar on tablet (768px)` | Sidebar collapses to icons |
| 5 | `should show drawer on mobile (480px)` | Hamburger menu + drawer |

## i18n (2 tests, F17 fix — separate context + reset)
| # | Test | Expected |
|---|------|----------|
| 6 | `should switch to English` | Labels change to EN |
| 7 | `should switch back to Vietnamese` | Labels change to VN |

**Note:** Tests run in isolated browser context. `afterEach` resets to Vietnamese to prevent contamination (F17).

## Theme (1 test)
| # | Test | Expected |
|---|------|----------|
| 8 | `should toggle dark mode` | Dark palette applied (data-theme="dark") |

## Accessibility — axe-core (4 tests, F14 fix)

Install `@axe-core/playwright`. Scan major page types for WCAG 2.1 AA violations.

| # | Test | Page Scanned |
|---|------|-------------|
| 9 | `a11y: project list page` | /domestic-projects |
| 10 | `a11y: project form page` | /domestic-projects/new |
| 11 | `a11y: admin catalog page` | /admin/catalogs |
| 12 | `a11y: dashboard` | / |

**Severity threshold:** Fail on `critical` + `serious` only. `moderate` + `minor` logged, don't fail.

## Performance Baselines (2 tests, F15 fix)

| # | Test | Metric | Threshold |
|---|------|--------|-----------|
| 13 | `project list load < 3s` | navigation → table visible | < 3000ms |
| 14 | `project form load < 5s` | navigation → form rendered (8+ API calls) | < 5000ms |

## Files to Create
- `e2e/smoke/navigation.spec.ts`
- `e2e/smoke/responsive.spec.ts`
- `e2e/smoke/i18n.spec.ts`
- `e2e/smoke/theme.spec.ts`
- `e2e/smoke/accessibility.spec.ts`
- `e2e/smoke/performance-baselines.spec.ts`

## Dependencies
- `@axe-core/playwright` — npm install as devDependency
