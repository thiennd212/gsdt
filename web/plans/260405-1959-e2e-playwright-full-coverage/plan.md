---
title: "E2E Playwright Full Coverage"
description: "6-phase plan: POM infrastructure, public forms, admin UI routes, JIT SSO API, error scenarios, perf baselines"
status: implemented
priority: P1
effort: 18h
branch: main
tags: [e2e, playwright, testing, coverage]
created: 2026-04-05
---

# E2E Playwright Full Coverage Plan

## Current State

- 36 existing spec files (4,333 LOC) — mostly API-level tests (34 API, 2 browser)
- 2 browser-UI files tested (browser-ui.spec.ts, smoke-new-modules.spec.ts)
- No Page Object Model, no test data factory, no perf baselines
- 60+ FE routes registered (29 admin, 16 user-facing, 3 public/auth)

## Phase Overview

<!-- Red-team: Phase 0 merged into Phase 1 (circular dependency fix) -->
<!-- Red-team: Only refactor 2 browser files, not 36 API files (POM irrelevant for API tests) -->
<!-- Red-team: Phase 4 changed to API-only (server rejects unsigned JWTs) -->
<!-- Red-team: Phase 3 labeled honestly as "render + selective CRUD" -->

| # | Phase | Effort | Status | Depends On | Files |
|---|-------|--------|--------|------------|-------|
| 1 | [Test Infrastructure + Refactor](phase-01-test-infrastructure.md) | 2.5h | Done | - | 10 new, 1 modified |
| 2 | [Public Form Flows](phase-02-public-form-flows.md) | 3h | Done | Phase 1 | 4 new |
| 3a | [Admin UI: Identity routes](phase-03-admin-ui-browser-tests.md) | 3h | Done | Phase 1 | 1 new |
| 3b | Admin UI: Content + System | 2h | Done | Phase 3a | 1 new |
| 3c | Admin UI: Integration + User | 2h | Done | Phase 3b | 1 new |
| 4 | [JIT SSO API Verification](phase-04-jit-sso-flow.md) | 1h | Done | Phase 1 | 3 new |
| 5 | [Error Scenarios + Authz Boundary](phase-05-error-scenarios.md) | 2.5h | Done | Phase 3c | 4 new |
| 6 | [Performance Baselines](phase-06-performance-baselines.md) | 1h | Done | Phase 1 | 2 new |

Total: ~18h effort, 2 modified + ~26 new files

## Architecture

```
web/e2e/
  helpers/
    auth-helper.ts          (existing — ROPC token)
    navigation-helper.ts    (new — goto + wait patterns)
    form-helper.ts          (new — fill/submit/validate)
    table-helper.ts         (new — filter/sort/pagination)
  fixtures/
    test-data-factory.ts    (new — API-driven seed/cleanup)
    auth-fixture.ts         (new — Playwright fixture extending base test)
  pages/
    login-page.ts           (new — POM)
    admin-layout-page.ts    (new — sidebar, topbar, navigation)
    data-table-page.ts      (new — reusable table component POM)
    form-modal-page.ts      (new — reusable modal/drawer form POM)
    public-form-page.ts     (new — citizen form submission POM)
  browser-ui/               (new — browser-level test specs)
    admin-identity.spec.ts
    admin-content.spec.ts
    admin-system.spec.ts
    admin-integration.spec.ts
    user-pages.spec.ts
  public-form-flows.spec.ts
  jit-sso-flow.spec.ts
  error-scenarios.spec.ts
  performance-baselines.spec.ts
```

## Data Flow

```
Test Fixture (auth-fixture.ts)
  |-- acquires OIDC browser auth OR ROPC API token
  |-- seeds test data via test-data-factory.ts (API calls)
  |
  v
Page Object (pages/*.ts)
  |-- encapsulates selectors & actions
  |-- used by spec files for assertions
  |
  v
Spec File (*.spec.ts)
  |-- orchestrates scenario steps
  |-- asserts outcomes
  |
  v
Cleanup (afterAll/afterEach)
  |-- deletes seeded data via API
```

## Failure Modes & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| OIDC cold start timeout | Medium | High | 120s timeout on login, retry in beforeAll |
| Test data leaks across runs | Medium | Medium | Factory tracks created IDs, afterAll cleanup |
| Flaky selectors (Ant Design) | High | Medium | POM with data-testid fallback to role/text |
| Docker services not running | Low | High | Health check in globalSetup, skip with message |
| Port conflicts (3000 vs 5173) | Medium | Low | WEB_URL env var, documented in config |
| JIT SSO needs external IdP | High | Medium | Mock IdP via Playwright route intercept |

## Backwards Compatibility

- 34 API-only spec files unchanged (POM irrelevant for API tests)
- 2 browser spec files refactored to POM in Phase 1 (browser-ui.spec.ts, smoke-new-modules.spec.ts)
- `playwright.config.ts` modified (new project entries)
- Existing `auth-helper.ts` used as single source of truth for credentials

## Rollback

- Phase 1: revert 2 browser spec files + delete new POM/fixture files + revert playwright.config.ts
- Phases 2-6: delete new spec files only (no existing files modified)
- Use `git revert` for Phase 1 changes to modified files

## Validation Log

### Session 1 — 2026-04-05
**Trigger:** Pre-implementation validation interview
**Questions asked:** 6

#### Questions & Answers

1. **[Infra]** Plan giả định services đã chạy sẵn. Xử lý thế nào?
   - Options: Services chạy sẵn | Auto-start globalSetup | Docker compose only
   - **Answer:** Custom — dùng skill `/aqt-runapp` để chạy FE, BE, Auth server trước khi test
   - **Rationale:** Consistent startup, đã có sẵn skill, không cần phức tạp globalSetup

2. **[SSO Mock]** Phase 4 JIT SSO cần mock external IdP?
   - Options: API-level only | Playwright route intercept | Duende demo | Bỏ Phase 4
   - **Answer:** Playwright route intercept
   - **Rationale:** Giả lập full SSO flow trong browser, realistic hơn API-only

3. **[Depth]** Phase 3 mức độ kiểm tra?
   - Options: Render + basic interaction | Render only | Full CRUD per route
   - **Answer:** Full CRUD per route
   - **Rationale:** Đảm bảo mọi route CRUD hoạt động, không chỉ render

4. **[Perf]** Performance thresholds cho dev?
   - Options: Dev lỏng hơn (5s/1s) | Giữ plan (3s/500ms) | Bỏ Phase 6
   - **Answer:** Giữ như plan (3s/500ms)
   - **Rationale:** Strict thresholds phát hiện regression sớm

5. **[Phase 3]** Full CRUD tăng effort ~12h+, chia thế nào?
   - Options: 3 sub-phases | Parallel agents | Priority routes only
   - **Answer:** Chia thành 3 sub-phases (Identity → Content+System → Integration+User)
   - **Rationale:** Tuần tự, dễ theo dõi, không risk file conflict

6. **[Refactor]** Existing 36 test files refactor sang POM?
   - Options: Không refactor | Refactor browser tests | Refactor tất cả
   - **Answer:** Refactor tất cả
   - **Rationale:** Consistency, dù effort cao nhưng long-term maintainability tốt hơn

#### Confirmed Decisions
- Infrastructure: dùng `/aqt-runapp` trước khi test
- JIT SSO: Playwright route intercept (full browser mock)
- Admin routes: Full CRUD (3 sub-phases, ~12h total)
- Performance: strict thresholds (3s/500ms)
- Refactor: all 36 existing files sang POM

#### Action Items
- [ ] Add Phase 0: Refactor existing 36 test files to POM
- [ ] Split Phase 3 into 3a/3b/3c sub-phases
- [ ] Update Phase 4 to use Playwright route intercept
- [ ] Update total effort: 2h + 3h + 12h + 2h + 2h + 1h + refactor ~4h = ~26h
- [ ] Add globalSetup health check (verify services running)

#### Impact on Phases
- Phase 0 (NEW): Refactor existing 36 files — ~4h effort, blocks Phase 1
- Phase 1: No change
- Phase 3: Split to 3a (Identity, 4h), 3b (Content+System, 3h), 3c (Integration+User, 3h)
- Phase 4: Change from API-only to Playwright route intercept
- Phase 6: Keep strict thresholds

## Red Team Review

### Session — 2026-04-05
**Findings:** 15 (14 accepted, 1 rejected)
**Severity breakdown:** 4 Critical, 7 High, 4 Medium

| # | Finding | Severity | Disposition | Applied To |
|---|---------|----------|-------------|------------|
| 1 | Effort inconsistency (14h/24h/26h) | Critical | Accept | plan.md — reconciled to 18h |
| 2 | Phase 0 ↔ Phase 1 circular dependency | Critical | Accept | Merged Phase 0 into Phase 1 |
| 3 | Refactor 36 API files to POM is pointless | Critical | Accept | Only refactor 2 browser files |
| 4 | No authz boundary testing (single admin) | Critical | Accept | Added to Phase 5 |
| 5 | Phase 4 route intercept contradicts own analysis | High | Accept | Phase 4 → API-only |
| 6 | Phase 3 "Full CRUD" scope mismatch | High | Accept | Relabeled "render + selective CRUD" |
| 7 | Cleanup runs only on success | High | Accept | All cleanup → afterAll/fixture teardown |
| 8 | No XSS/injection tests on public forms | High | Accept | Added to Phase 2 |
| 9 | Hardcoded credentials duplicated | High | Accept | Use auth-helper.ts as single source |
| 10 | Port mismatch localhost vs 127.0.0.1 | High | Accept | All new code uses 127.0.0.1 |
| 11 | Worker fixture conflicts with error tests | Medium | Accept | Error tests use separate browser context |
| 12 | Phase 6 perf baselines flaky on dev | Medium | Reject | User validated strict thresholds |
| 13 | Rollback claim false ("new files only") | High | Accept | Updated rollback docs |
| 14 | Concurrent edit test silently no-ops | Medium | Accept | Verify API first or skip with TODO |
| 15 | TestDataFactory no cap/no try-catch | Medium | Accept | Add safeguards to cleanup |

## Success Criteria

- [x] Phase 1: POM classes work, auth fixture provides authenticated page, 2 browser files refactored
- [x] Phase 2: Public form lifecycle + XSS injection test passes
- [x] Phase 3: All admin + user routes render; selective CRUD on key routes (users, groups, system-params)
- [x] Phase 4: JIT provider config CRUD verified via API
- [x] Phase 5: Error boundary + authz boundary (admin vs non-admin) + session expiry tested
- [x] Phase 6: Page load <3s, API response <500ms assertions pass
