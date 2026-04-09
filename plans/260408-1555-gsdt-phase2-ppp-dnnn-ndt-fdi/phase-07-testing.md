# Phase 07 — Testing

## Context Links
- Phase 1 E2E tests: `tests/e2e/` (120 tests, all passing)
- Phase 1 test pattern: `plans/260407-1007-gsdt-phase1-dtc-crud/phase-09-testing.md`
- Recent test commits: `e65dbbd`, `9bc90c8`, `914b2b5`, `4eb046c`, `0c3a056`

## Overview
- **Priority:** P2
- **Status:** Complete
- **Effort:** 3-5 days
- **Blocker:** None (all dependencies resolved 2026-04-09)
- **Description:** Unit tests for BE domain logic + validators. E2E tests for all 4 new project types (PPP, DNNN, NĐT, FDI) covering CRUD, sub-entity operations, list/filter, and admin catalog CRUD (GovernmentAgency, Investor).

## Key Insights
- Phase 1 established E2E test patterns — follow same structure
- 120 E2E tests already exist and pass — new tests must not break them
- PPP has the most test surface: unique Tab2 (investor selection + contract info), revenue reports, 3-source disbursement, design estimates
- DNNN adds: GCNĐKĐT (certificates), DNNN capital validation
- NĐT/FDI: thin clones — test coverage can be lighter (CRUD + certificates)
- BE unit tests: focus on validators (capital breakdown validation) and factory methods
- Admin catalogs: GovernmentAgency (hierarchical CRUD + tree), Investor (flat CRUD)

## Requirements

### Functional
1. BE unit tests for validators: PPP capital (VonNN+CSH+Vay), DNNN capital (CSH+ODA+TCTD), DesignEstimate sum
2. BE unit tests for entity factory methods: all 4 new project types
3. E2E tests for PPP: CRUD, all sub-entities, list with filters
4. E2E tests for DNNN: CRUD, certificates, decisions, list with filters
5. E2E tests for NĐT/FDI: CRUD, certificates, basic sub-entities
6. E2E tests for admin catalogs: GovernmentAgency tree CRUD, Investor CRUD
7. E2E tests for existing catalog updates: Province/Ward status fields

### Non-Functional
- All tests must be independent (no test-to-test state leaks)
- E2E tests use authenticated API client
- Test data cleanup after each test suite

## Architecture

### Test Matrix

| Category | Scope | Count (est.) | Priority |
|----------|-------|-------------|----------|
| **BE Unit — Validators** | | | |
| PPP capital validation | Total == State+CSH+Loan | 4 | P1 |
| PPP state capital breakdown | StateCapital == NSTW+NSĐP+Other | 3 | P1 |
| DNNN capital validation | Total == CSH+ODA+TCTD | 4 | P1 |
| DesignEstimate sum validation | Total == 7 cost items | 3 | P1 |
| EquityRatio auto-calc | CSH/TMĐT, edge cases (0 divisor) | 3 | P2 |
| Revenue report period validation | Year range, period values | 2 | P2 |
| **BE Unit — Factories** | | | |
| PppProject.Create | Sets ProjectType, Id, raises event | 2 | P1 |
| DnnnProject.Create | Same pattern | 2 | P1 |
| NdtProject.Create, FdiProject.Create | Same pattern | 2 | P2 |
| RegistrationCertificate.Create | Fields set correctly | 1 | P2 |
| **E2E — PPP** | | | |
| PPP CRUD (create/read/update/delete) | Full lifecycle | 4 | P1 |
| PPP decisions (add/delete) | PPP capital structure | 2 | P1 |
| PPP investor selection (upsert) | Multiselect investors | 2 | P1 |
| PPP contract info (upsert) | TMĐT breakdown | 2 | P1 |
| PPP capital plans (add/delete) | | 2 | P2 |
| PPP design estimates (add/update/delete) | 7 cost items + items | 3 | P1 |
| PPP disbursements (add/delete) | 3-source | 2 | P2 |
| PPP revenue reports (add/update/delete) | Periodic | 3 | P1 |
| PPP locations (add/delete) | | 2 | P2 |
| PPP documents (add/delete) | | 2 | P2 |
| PPP bid packages (add/delete) | Reuse pattern | 2 | P2 |
| PPP list with filters | Search, contract type, status | 3 | P1 |
| **E2E — DNNN** | | | |
| DNNN CRUD | Full lifecycle | 4 | P1 |
| DNNN decisions | DNNN capital | 2 | P1 |
| DNNN certificates (add/update/delete) | GCNĐKĐT | 3 | P1 |
| DNNN investor selection | Reuse | 2 | P2 |
| DNNN design estimates | Reuse | 2 | P2 |
| DNNN list with filters | 5 filters | 2 | P1 |
| **E2E — NĐT** | | | |
| NĐT CRUD | Full lifecycle | 4 | P1 |
| NĐT decisions + certificates | Core sub-entities | 3 | P1 |
| NĐT list | Basic | 1 | P2 |
| **E2E — FDI** | | | |
| FDI CRUD | Full lifecycle | 4 | P1 |
| FDI decisions + certificates | Core sub-entities | 3 | P1 |
| FDI list | Basic | 1 | P2 |
| **E2E — Admin Catalogs** | | | |
| GovernmentAgency CRUD | Create/read/update/delete | 4 | P1 |
| GovernmentAgency tree | Hierarchy (parent-child) | 2 | P1 |
| GovernmentAgency validation | Duplicate code, self-parent | 2 | P2 |
| Investor CRUD | Create/read/update/delete | 4 | P1 |
| Investor type filter | Filter by InvestorType | 1 | P2 |
| Province/Ward status | Verify new fields | 2 | P2 |
| **Smoke / Regression** | | | |
| Existing TN CRUD still works | No regression | 1 | P1 |
| Existing ODA CRUD still works | No regression | 1 | P1 |
| **Total** | | **~95-105** | |

### Test File Structure

```
tests/
├── unit/
│   └── investment-projects/
│       ├── PppCapitalValidationTests.cs
│       ├── DnnnCapitalValidationTests.cs
│       ├── DesignEstimateSumValidationTests.cs
│       ├── EquityRatioCalculationTests.cs
│       └── ProjectFactoryTests.cs
└── e2e/
    ├── ppp-projects/
    │   ├── ppp-crud.spec.ts
    │   ├── ppp-decisions.spec.ts
    │   ├── ppp-investor-selection.spec.ts
    │   ├── ppp-contract-info.spec.ts
    │   ├── ppp-design-estimates.spec.ts
    │   ├── ppp-revenue-reports.spec.ts
    │   ├── ppp-disbursements.spec.ts
    │   ├── ppp-sub-entities.spec.ts         # locations, documents, bid-packages, capital plans, execution
    │   └── ppp-list-filters.spec.ts
    ├── dnnn-projects/
    │   ├── dnnn-crud.spec.ts
    │   ├── dnnn-decisions.spec.ts
    │   ├── dnnn-certificates.spec.ts
    │   ├── dnnn-sub-entities.spec.ts
    │   └── dnnn-list-filters.spec.ts
    ├── ndt-projects/
    │   ├── ndt-crud.spec.ts
    │   └── ndt-sub-entities.spec.ts
    ├── fdi-projects/
    │   ├── fdi-crud.spec.ts
    │   └── fdi-sub-entities.spec.ts
    ├── admin-catalogs/
    │   ├── government-agency-crud.spec.ts
    │   ├── government-agency-tree.spec.ts
    │   ├── investor-crud.spec.ts
    │   └── province-ward-status.spec.ts
    └── regression/
        └── tn-oda-regression.spec.ts
```

## Implementation Steps

### BE Unit Tests (1d)

1. Create `PppCapitalValidationTests.cs`:
   - Valid: Total == State + CSH + Loan → passes
   - Invalid: Total mismatch → fails with specific error
   - Edge: all zeros → passes
   - Edge: negative values → fails

2. Create `DnnnCapitalValidationTests.cs`:
   - Same pattern: Total == CSH + ODA + TCTD

3. Create `DesignEstimateSumValidationTests.cs`:
   - Valid: Total == sum of 7 items
   - Invalid: mismatch
   - Edge: one item zero

4. Create `EquityRatioCalculationTests.cs`:
   - Normal: CSH / TMĐT rounded to 2 decimals
   - Edge: TMĐT == 0 → ratio is null/0
   - Edge: CSH > TMĐT → validation error

5. Create `ProjectFactoryTests.cs`:
   - PppProject.Create: sets ProjectType.Ppp, generates Id, raises event
   - DnnnProject.Create: sets ProjectType.Dnnn
   - NdtProject.Create, FdiProject.Create

### E2E — PPP (1.5d)

6. Create `ppp-crud.spec.ts`:
   - Create PPP project → 201 + returned id
   - Get by id → 200 + correct data
   - Update → 200 + fields changed
   - Delete → 204 + get returns 404

7. Create `ppp-decisions.spec.ts`:
   - Add decision with PPP capital → 200
   - Verify capital structure in response (stateCapital, equityCapital, loanCapital)
   - Delete decision → 204

8. Create `ppp-investor-selection.spec.ts`:
   - Upsert investor selection with multiple investorIds → 200
   - Get project → investorSelection populated with correct data
   - Upsert again (update) → verify changes persisted

9. Create `ppp-contract-info.spec.ts`:
   - Upsert contract info with TMĐT breakdown → 200
   - Verify auto-calc: stateCapital == NSTW + NSĐP + other
   - Verify equityRatio auto-calc

10. Create `ppp-design-estimates.spec.ts`:
    - Add estimate with 7 cost items → verify totalEstimate auto-sum
    - Update → verify new total
    - Delete → 204

11. Create `ppp-revenue-reports.spec.ts`:
    - Add revenue report → 200
    - Update with new period data → verify cumulative
    - Delete → 204

12. Create `ppp-sub-entities.spec.ts`:
    - Locations add/delete
    - Documents add/delete
    - Bid packages add/delete
    - Capital plans add/delete
    - Disbursements add/delete (verify 3-source fields)
    - Execution records add

13. Create `ppp-list-filters.spec.ts`:
    - Create 3 PPP projects with different attributes
    - Filter by contractType → correct subset
    - Filter by search text → correct subset
    - Filter by status → correct subset

### E2E — DNNN (0.5d)

14. Create `dnnn-crud.spec.ts` — same 4-test pattern
15. Create `dnnn-decisions.spec.ts` — DNNN capital (CSH/ODA/TCTD)
16. Create `dnnn-certificates.spec.ts` — Add/Update/Delete GCNĐKĐT, verify equityRatio
17. Create `dnnn-sub-entities.spec.ts` — investor selection, design estimates, locations, documents
18. Create `dnnn-list-filters.spec.ts` — 5 filters

### E2E — NĐT/FDI (0.5d)

19. Create `ndt-crud.spec.ts` + `ndt-sub-entities.spec.ts` — CRUD + certificates + decisions
20. Create `fdi-crud.spec.ts` + `fdi-sub-entities.spec.ts` — same pattern

### E2E — Admin Catalogs (0.5d)

21. Create `government-agency-crud.spec.ts` — CRUD
22. Create `government-agency-tree.spec.ts` — Create parent → create child → GET tree → verify hierarchy
23. Create `investor-crud.spec.ts` — CRUD + type filter
24. Create `province-ward-status.spec.ts` — Verify new EffectiveDate/Status fields exist

### Regression (0.5d)

25. Create `tn-oda-regression.spec.ts`:
    - Create domestic project → success
    - Create ODA project → success
    - List domestic → returns results
    - List ODA → returns results
    - Verify no 500 errors

## Todo Checklist

### BE Unit Tests
- [x] PppCapitalValidationTests (4 tests)
- [x] DnnnCapitalValidationTests (4 tests)
- [x] DesignEstimateSumValidationTests (3 tests)
- [x] EquityRatioCalculationTests (3 tests)
- [x] ProjectFactoryTests (4 tests)

### E2E Tests
- [x] PPP: crud, decisions, investor-selection, contract-info (P1)
- [x] PPP: design-estimates, revenue-reports, sub-entities (P1/P2)
- [x] PPP: list-filters (P1)
- [x] DNNN: crud, decisions, certificates, sub-entities, list-filters
- [x] NĐT: crud, sub-entities
- [x] FDI: crud, sub-entities
- [x] Admin: government-agency CRUD + tree
- [x] Admin: investor CRUD
- [x] Admin: province/ward status
- [x] Regression: TN + ODA still work

### Verification
- [x] All new tests pass
- [x] All existing 120 tests still pass
- [x] No test-to-test state leaks
- [x] CI pipeline green

## Success Criteria
- ~95-105 new tests written and passing
- All 120 existing Phase 1 tests remain green
- BE unit tests cover capital validation edge cases (zero, negative, mismatch)
- E2E tests cover full CRUD lifecycle for all 4 new project types
- Admin catalog tests cover hierarchy + flat CRUD
- CI pipeline passes with all tests

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Test data conflicts between new and old tests | Low | Medium | Use unique project codes per test suite; cleanup in afterAll |
| E2E tests slow due to 6 TPT types | Medium | Low | Parallelize test suites; test thin types (NĐT/FDI) minimally |
| Flaky tests from concurrent DB access | Low | Medium | Use isolated tenant IDs per test suite |
| Unit test mocking of EF context | Low | Low | Test validators directly (no DB needed); test factories as pure functions |
