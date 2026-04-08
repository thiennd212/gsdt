---
title: "E2E Playwright Browser Tests — Full Module Coverage"
description: "Comprehensive E2E test suite using Playwright for all GSDT FE modules"
status: complete
priority: P1
effort: 17-20d
branch: feature/e2e-playwright-tests
tags: [testing, e2e, playwright, quality]
created: 2026-04-08
updated: 2026-04-08
blockedBy: []
blocks: []
red-team-score: 4/10
red-team-date: 2026-04-08
---

# E2E Playwright Browser Tests — Full Module Coverage

**Effort:** 17-20 days (12-15d base + 5d expanded admin scope)
**Branch:** `feature/e2e-playwright-tests`
**Prereqs:** All services running (BE 6001, Auth 6002, FE 6173), test users seeded
**Red-team:** 18 findings addressed | **Validated:** 2026-04-08

---

## Phases

| # | Phase | Status | Effort | Tests |
|---|-------|--------|--------|-------|
| 0 | [Prep: data-testid + FE fixes](phase-00-prep-testid-fe-fixes.md) | Complete | 2d | — |
| 1 | [Setup & Auth Fixtures](phase-01-setup-auth-fixtures.md) | Complete | 1.5d | 3 |
| 2 | [Core GSDT — Domestic Projects](phase-02-domestic-projects-e2e.md) | Complete | 2d | 20 |
| 3 | [Core GSDT — ODA Projects](phase-03-oda-projects-e2e.md) | Complete | 1.5d | 16 |
| 4 | [Admin Catalogs](phase-04-admin-catalogs-e2e.md) | Complete | 0.5d | 10 |
| 5 | [Identity & Access](phase-05-identity-access-e2e.md) | Complete | 1d | 12 |
| 6 | [System Modules + Remaining](phase-06-system-modules-e2e.md) | Complete | 1d | 15 |
| 7 | [Admin Modules CRUD](phase-08-admin-modules-crud-e2e.md) | Complete | 3d | 30 |
| 8 | [Cross-Cutting, Smoke, a11y, Perf](phase-07-cross-cutting-smoke-e2e.md) | Complete | 1.5d | 14 |
| **Total** | | **Complete** | **17-20d** | **~120 tests** |

## Resolved Questions (from red-team)

| Question | Decision | Rationale |
|----------|----------|-----------|
| Test DB | **Dev DB (shared)** with run-ID prefix isolation | User decision: avoid Docker setup overhead. Cleanup via globalTeardown |
| Auth | **TestTokenController** + sessionStorage injection | BE endpoint `/api/v1/test/token?role={role}` (Testing env only). F1/F2 fix |
| File upload | **Dev MinIO** (shared with dev env) | No separate Docker needed since using dev DB |
| Selector strategy | **data-testid > role > label > text** | Zero data-testid currently exists — Phase 0 adds them (F3) |
| Existing PW projects | **Preserve** existing 7 projects, add new `gsdt-e2e` project | Avoid breaking prior compliance config (F6) |
| Auth enforcement | **BE-enforced** (API 403) — FE shows all buttons | Test error toast on unauthorized click, not hidden buttons (F16) |
| Filter dropdowns | **Fix in Phase 0** — wire onChange to parent state | Dropdowns are currently decorative (F4) |

## Test Architecture (revised)

```
web/e2e/
├── global-setup.ts                # Health-check services, seed test users
├── global-teardown.ts             # Cleanup test data
├── fixtures/
│   ├── auth.fixture.ts            # sessionStorage token injection per role (F1/F2)
│   ├── test-data.fixture.ts       # API helpers: seed/cleanup with run-ID prefix (F5)
│   ├── ant-helpers.ts             # Ant Design interaction: Select, DatePicker, Modal, Popconfirm (F13)
│   └── page-objects/
│       ├── domestic-project.po.ts
│       ├── oda-project.po.ts
│       ├── catalog.po.ts
│       ├── user-management.po.ts
│       └── common.po.ts           # Table, form, toast, breadcrumb helpers
├── domestic-projects/             # P02: 20 tests (15 happy + 5 negative)
├── oda-projects/                  # P03: 16 tests (12 happy + 4 negative)
├── admin-catalogs/                # P04: 10 tests
├── identity/                      # P05: 12 tests
├── system/                        # P06: 15 tests (covers remaining 26 modules via smoke)
└── smoke/                         # P07: 14 tests (nav, responsive, i18n, a11y, perf)
```

## Selector Strategy (F3)

| Priority | Method | Example |
|----------|--------|---------|
| 1 | `data-testid` | `page.getByTestId('btn-save-tab1')` |
| 2 | ARIA role | `page.getByRole('button', { name: 'Lưu thông tin' })` |
| 3 | Label | `page.getByLabel('Mã dự án')` |
| 4 | Text (last resort) | `page.getByText('Thêm mới')` |
| NEVER | CSS class | ~~`.ant-select-selector`~~ |

## Auth Strategy (F1/F2)

```typescript
// e2e/fixtures/auth.fixture.ts — inject sessionStorage token
async function loginAs(page: Page, role: 'BTC' | 'CQCQ' | 'CDT') {
  const token = await fetchTestToken(role); // call BE /test-token endpoint
  await page.goto(baseURL);
  await page.evaluate((t) => {
    sessionStorage.setItem(
      'oidc.user:http://localhost:6002:gsdt-spa-dev',
      JSON.stringify(t)
    );
  }, token);
  await page.reload();
  await page.waitForSelector('[data-testid="sidebar-menu"]');
}
```

## Test Data Isolation (F5)

- All test entities prefixed with `e2e-{runId}-` (runId = timestamp)
- `globalTeardown` deletes all entities with `e2e-` prefix via API
- Each spec file uses `test.afterAll()` for immediate cleanup
- Separate test DB: `GSDT_Test` via Docker Compose

## Ant Design Helpers (F13)

```typescript
// e2e/fixtures/ant-helpers.ts
async function antSelect(page, testId, optionText) { ... }
async function antDatePick(page, testId, date) { ... }
async function antConfirmPopup(page) { ... }
async function antWaitModalClose(page) { ... }
// Global CSS injected in test mode to disable animations:
// * { transition: none !important; animation: none !important; }
```

## Coverage Matrix (revised — F8)

### P0: Full E2E (3 modules, ~46 tests)
| Module | Tests | Includes |
|--------|-------|----------|
| Domestic Projects | 20 | CRUD + tabs + filters + auth + validation + concurrency |
| ODA Projects | 16 | CRUD + ODA fields + auth + validation |
| Admin Catalogs | 10 | Generic + KHLCNT + auth + index |

### P1: Functional E2E (3 modules, ~27 tests)
| Module | Tests | Includes |
|--------|-------|----------|
| Identity & Access | 12 | Login + users + roles + profile |
| System Modules | 15 | Dashboard + files + audit + org + params + smoke for 20 remaining |

### P2: Cross-cutting (14 tests)
| Category | Tests |
|----------|-------|
| Navigation smoke | 3 |
| Responsive | 2 |
| i18n | 2 |
| Theme | 1 |
| **a11y (axe-core)** | 4 (F14) |
| **Performance baselines** | 2 (F15) |

## Negative Tests Added (F7)

| Module | Negative Test | Expected |
|--------|--------------|----------|
| Domestic | Submit Tab 1 with empty required fields | Inline validation errors |
| Domestic | Duplicate project code | API 409 → error toast |
| Domestic | Concurrent edit (RowVersion conflict) | API 409 → "Dữ liệu đã bị thay đổi" toast (F9) |
| Domestic | Tab 1 save fails → Tab 2 state preserved | Tab 2 form not cleared (F10) |
| Domestic | Create → edit redirect → Tabs 2-6 enabled | Tabs become active after redirect (F10) |
| ODA | Mechanism % ≠ 100 | Validation error on save |
| ODA | Empty QHNS accepted (v1.1 positive) | No error |
| ODA | Duplicate project code | API 409 → error toast |
| ODA | CDT tries to edit other's project | API 403 → error toast (F16) |
| Catalogs | Duplicate catalog code | API 409 → error toast |

## Key Decisions (revised)

| Decision | Rationale |
|----------|-----------|
| **Phase 0 prerequisite** | Add data-testid + fix filter dropdowns before any test (F3/F4) |
| **sessionStorage injection** | Playwright storageState doesn't capture sessionStorage (F1/F2) |
| **Run-ID prefixed test data** | Prevent parallel/re-run pollution (F5) |
| **Ant Design animation disable** | CSS injection `transition: none` prevents flakiness (F13) |
| **axe-core for a11y** | GOV compliance requirement (F14) |
| **Separate test DB** | Clean isolation from dev data (F5) |
| **BE-enforced auth testing** | FE doesn't hide buttons — test API 403 + error toast (F16) |
| **Preserve existing PW config** | 7 existing projects remain, add `gsdt-e2e` project (F6) |

## Red-Team Findings Disposition

| # | Finding | Severity | Disposition |
|---|---------|----------|-------------|
| F1 | OIDC sessionStorage | Critical | **Fixed** — Phase 1 uses page.evaluate() injection |
| F2 | storageState gap | Critical | **Fixed** — same as F1 |
| F3 | Zero data-testid | Critical | **Fixed** — Phase 0 adds testid to all modules |
| F4 | Filter dropdowns non-functional | High | **Fixed** — Phase 0 wires onChange handlers |
| F5 | No data cleanup | High | **Fixed** — run-ID prefix + globalTeardown |
| F6 | Existing PW config ignored | High | **Fixed** — preserve + add gsdt-e2e project |
| F7 | No negative tests | High | **Fixed** — 10 negative tests added across P02-P04 |
| F8 | 26 modules uncovered | High | **Fixed** — P06 adds smoke for all remaining modules |
| F9 | No concurrency test | High | **Fixed** — concurrent edit test in P02 |
| F10 | Tab cross-persistence | Medium | **Fixed** — 2 tests in P02 for create→edit + tab state |
| F11 | No webServer/service deps | Medium | **Fixed** — globalSetup health-checks, Docker Compose |
| F12 | Effort unrealistic | Medium | **Fixed** — 5-7d → 12-15d |
| F13 | No Ant Design strategy | Medium | **Fixed** — ant-helpers.ts with animation disable |
| F14 | No a11y | Medium | **Fixed** — 4 axe-core tests in P07 |
| F15 | No perf baselines | Medium | **Fixed** — 2 perf tests in P07 |
| F16 | Auth FE vs BE | Medium | **Fixed** — test API 403 + error toast |
| F17 | i18n test ordering | Low | **Fixed** — separate context + reset to VN |
| F18 | File upload MinIO | Low | **Fixed** — MinIO in Docker Compose |
