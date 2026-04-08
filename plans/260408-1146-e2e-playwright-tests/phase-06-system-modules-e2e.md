# Phase 06 — System Modules + Remaining Smoke E2E

**Status:** Pending | **Effort:** 1d | **Tests:** 15 | **Depends on:** P01
**Red-team:** Addresses F8 (26 uncovered modules — smoke tests added)

## Functional Tests (10)

### Dashboard (2)
| # | Test | Expected |
|---|------|----------|
| 1 | `should render dashboard with stats` | KPI cards visible |
| 2 | `should navigate from dashboard cards` | Click → navigates |

### Files (2)
| # | Test | Expected |
|---|------|----------|
| 3 | `should list files` | Table renders |
| 4 | `should upload file (MinIO)` | Upload → appears in list |

### Audit + Notifications (2)
| # | Test | Expected |
|---|------|----------|
| 5 | `should list audit logs with date filter` | Table + filter works |
| 6 | `should list notifications` | Notification list renders |

### Organization + System Params (2)
| # | Test | Expected |
|---|------|----------|
| 7 | `should show org unit tree` | Tree expands/collapses |
| 8 | `should edit system param` | Save → updated |

### Master Data + Roles (2)
| # | Test | Expected |
|---|------|----------|
| 9 | `should show province/district cascade` | Cascade selects work |
| 10 | `should show roles page` | Roles table visible |

## Smoke Tests for 20+ Remaining Modules (5, F8 fix)

Navigate to each module page, verify it renders without error.

| # | Test | Modules Covered |
|---|------|----------------|
| 11 | `should render identity admin pages` | groups, abac-rules, sod-rules, policy-rules, credential-policies, access-reviews, delegations, sessions, external-identities, jit-provider-config |
| 12 | `should render admin system pages` | admin-dashboard, jobs, health, backup, rtbf |
| 13 | `should render admin content pages` | templates, notification-templates, menus |
| 14 | `should render admin integration pages` | api-keys, webhooks, ai-admin, data-scopes |
| 15 | `should render user pages` | profile, consent |

Each smoke test navigates to the route, waits for page load, asserts no error boundary / 500 / blank page.

## Files to Create
- `e2e/system/dashboard.spec.ts`
- `e2e/system/files.spec.ts`
- `e2e/system/audit-notifications.spec.ts`
- `e2e/system/organization-params.spec.ts`
- `e2e/system/masterdata-roles.spec.ts`
- `e2e/system/remaining-modules-smoke.spec.ts`
