# Phase 08 — Buffer & Polish

## Context Links
- All prior phases: Phases 01-07
- Phase 1 buffer reference: `plans/260407-1007-gsdt-phase1-dtc-crud/phase-10-buffer-and-polish.md`
- Design decisions: `plans/reports/srs-analysis-260408-1527-gsdt-v12-delta.md` (section 14)

## Overview
- **Priority:** P3
- **Status:** Pending
- **Effort:** 2-3 days
- **Blocker:** Phase 7 (all tests passing)
- **Description:** Final polish, deferred items, integration verification, documentation updates, performance checks. Handle BA feedback on deferred items. Prepare for merge to main.

## Key Insights
- DNNN "Nghĩa vụ tài chính" (R80) is deferred — may need implementation if BA provides field spec
- NĐT/FDI field differences may be clarified by BA — plan for adjustments
- 6 TPT children total — need to verify EF query performance
- Shared component refactors (tab5-operation configurable hook) need regression verification
- GovernmentAgency migration from existing "managing-agencies" catalog — TN/ODA may need ref updates

## Requirements

### Functional
1. Address BA feedback on deferred items (Nghĩa vụ TC, NĐT/FDI differences)
2. Verify cross-type navigation (sidebar menu, breadcrumbs)
3. Verify all 6 project type list pages load correctly from dashboard
4. Polish UI: consistent spacing, loading states, error messages, empty states
5. Update documentation (roadmap, changelog, architecture)

### Non-Functional
- Performance: list queries for all 6 types < 500ms
- No console errors or warnings in browser
- All migrations apply cleanly on fresh database
- All migrations apply cleanly on existing Phase 1 database

## Architecture

### Deferred Items Backlog

| Item | Status | Action if BA Confirms |
|------|--------|----------------------|
| DNNN Nghĩa vụ TC (R80) | No field spec | Add text field section in DNNN Tab3 or new sub-tab |
| NĐT field differences from DNNN | "tương tự" | Adjust NdtProject entity + FE form fields |
| FDI label changes from NĐT | "tương tự" | Update Vietnamese labels in FDI FE |
| GovernmentAgency → TN/ODA migration | Completed in Phase 1 | Data migration from ManagingAuthority → GovernmentAgency done in Phase 1. TN/ODA already reference GovernmentAgency. Verify references in this phase only. |
| PPP Tab5 layout (separate tables vs inline) | SRS restructure noted | Adjust if BA requests specific layout |

### Performance Checklist

| Query | Target | Measurement |
|-------|--------|-------------|
| `GET /api/v1/ppp-projects?page=1&pageSize=50` | < 500ms | Swagger timing |
| `GET /api/v1/dnnn-projects?page=1&pageSize=50` | < 500ms | |
| `GET /api/v1/ndt-projects?page=1&pageSize=50` | < 500ms | |
| `GET /api/v1/fdi-projects?page=1&pageSize=50` | < 500ms | |
| `GET /api/v1/ppp-projects/{id}` (full detail) | < 1s | Includes all sub-entities |
| `GET /api/v1/masterdata/government-agencies/tree` | < 500ms | Hierarchical |
| Existing TN/ODA list queries | No regression | Compare with Phase 1 baseline |

### Migration Verification Matrix

| Scenario | Test |
|----------|------|
| Fresh DB (no data) | Apply all migrations from scratch |
| Existing Phase 1 DB | Apply Phase 2 migrations on top of existing data |
| Province/Ward with existing seed data | Verify new columns are nullable, existing data preserved |
| Rollback Phase 2 migrations | `dotnet ef migrations remove` in reverse order |

## Implementation Steps

### Integration Verification (0.5d)

1. Full fresh-DB migration test: drop DB → apply all migrations → verify schema
2. Incremental migration test: start from Phase 1 DB → apply Phase 2 migrations → verify
3. Verify all 6 project types can be created end-to-end (API + FE)
4. Verify sidebar navigation: all 6 project types accessible
5. Verify dashboard links (if any) to new project types

### Performance Check (0.5d)

6. Run all list queries with 100+ records per type → measure response time
7. Run detail queries with fully-populated projects (all sub-entities) → measure
8. Check EF query plans: ensure no N+1 queries in detail endpoints
9. Verify GovernmentAgency tree endpoint performance with 50+ nodes
10. Compare TN/ODA query times vs Phase 1 baseline (no regression)

### UI Polish (0.5d)

11. Verify all forms: loading states (skeleton), error states (red alerts), empty states ("Chua co du lieu")
12. Verify money inputs: locale formatting, decimal precision display
13. Verify file upload: .pdf validation, progress indicator, error feedback
14. Verify tab save indicators: saved (green check), unsaved (orange dot), idle (none)
15. Verify Popconfirm on all delete actions: consistent messaging
16. Verify responsive layout: 2-column forms collapse on mobile
17. Check browser console: no errors, no React key warnings, no unused state warnings

### Deferred Items (0.5-1d, if BA provides specs)

18. If BA provides Nghĩa vụ TC field spec:
    - Add fields to DNNN THTH tab (or create new sub-section)
    - Add API endpoint
    - Add FE form section
    - Add tests

19. If BA clarifies NĐT/FDI differences:
    - Update entity fields
    - Update FE form labels/fields
    - Add migration if schema changes

20. GovernmentAgency verification for TN/ODA:
    - Data migration from ManagingAuthority → GovernmentAgency was completed in Phase 1
    - Verify TN/ODA "Co quan quan ly" dropdowns correctly resolve to GovernmentAgency records
    - No additional migration needed — confirm integrity only

### Documentation (0.5d)

21. Update `docs/development-roadmap.md`:
    - Phase 2 status: Complete
    - Phase 2 deliverables summary
    - Phase 3 preview (if known)

22. Update `docs/project-changelog.md`:
    - Entry for Phase 2: 4 new project types, 2 new catalogs, entity counts
    - Breaking changes: none
    - Migration notes

23. Update `docs/system-architecture.md`:
    - Entity map: add 4 new TPT children
    - API routes: add new endpoints
    - Module diagram: update if needed

24. Update `docs/codebase-summary.md`:
    - File counts per module
    - New features summary

### Final Verification (0.5d)

25. Run full test suite: all ~215-225 tests (120 P1 + 95-105 P2)
26. Run `dotnet build` — zero warnings
27. Run `tsc` / `vite build` — zero errors
28. Git: clean working tree, all changes committed
29. Create PR description summarizing Phase 2

## Todo Checklist

### Integration
- [ ] Fresh DB migration test
- [ ] Incremental migration test (on Phase 1 DB)
- [ ] All 6 project types: end-to-end create flow
- [ ] Sidebar navigation: all types accessible
- [ ] Province/Ward: verify existing data preserved

### Performance
- [ ] List queries: all types < 500ms with 100+ records
- [ ] Detail queries: < 1s with all sub-entities
- [ ] GovernmentAgency tree: < 500ms
- [ ] No TN/ODA query regression
- [ ] Check EF query plans (no N+1)

### UI Polish
- [ ] Loading/error/empty states on all pages
- [ ] Money input formatting
- [ ] File upload validation + feedback
- [ ] Tab save indicators
- [ ] Delete confirmations
- [ ] No browser console errors

### Deferred (conditional)
- [ ] Nghĩa vụ TC (if BA provides spec)
- [ ] NĐT/FDI adjustments (if BA clarifies)
- [ ] GovernmentAgency → TN/ODA: verify references intact (migration completed in Phase 1)

### Documentation
- [ ] development-roadmap.md updated
- [ ] project-changelog.md updated
- [ ] system-architecture.md updated
- [ ] codebase-summary.md updated

### Final
- [ ] All tests green (~215-225)
- [ ] Zero build warnings/errors
- [ ] Clean git state
- [ ] PR ready

## Success Criteria
- All 6 project types: full CRUD works end-to-end (API + FE)
- All ~215-225 tests pass (zero failures)
- Performance targets met (< 500ms lists, < 1s details)
- No browser console errors
- Documentation updated
- Migrations apply cleanly on both fresh and existing databases
- PR created with comprehensive description

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| BA provides Nghĩa vụ TC spec late — adds scope | Medium | Medium | Timebox to 1d; if complex, defer to Phase 3 |
| Performance regression on TN/ODA queries | Low | High | Benchmark early in this phase; add DB indexes if needed |
| GovernmentAgency data migration for TN/ODA is complex | Medium | Medium | Defer to separate migration if risky |
| Fresh DB migration fails due to ordering | Low | Medium | Test early; fix migration order if needed |
