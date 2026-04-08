# Phase 03 — ODA Projects E2E

**Status:** Pending | **Effort:** 1.5d | **Tests:** 16 | **Depends on:** P00, P01
**Red-team:** Addresses F7 (negative), F16 (BE-enforced auth)

## Test Cases

### CRUD — Happy Path (4 tests)
| # | Test | Expected |
|---|------|----------|
| 1 | `should create ODA project with donor + mechanism %` | Created, grant+relending=100% |
| 2 | `should accept empty QHNS (v1.1)` | No validation error |
| 3 | `should edit ODA-specific fields` | Updated successfully |
| 4 | `should delete ODA project` | Soft-deleted, removed from list |

### Negative / Validation (4 tests, F7 fix)
| # | Test | Expected |
|---|------|----------|
| 5 | `should reject mechanism % ≠ 100` | Validation error on save |
| 6 | `should reject duplicate project code` | API 409 → error toast |
| 7 | `should show validation errors on empty submit` | Required field errors |
| 8 | `should handle concurrent edit` | API 409 → concurrency toast |

### ODA-Specific Fields (4 tests)
| # | Test | Expected |
|---|------|----------|
| 9 | `should show 12 status values in dropdown` | All 12 ODA statuses |
| 10 | `should toggle procurement condition` | Radio Có/Không → textarea toggles |
| 11 | `should auto-calculate total investment` | Sum of 5 capital fields |
| 12 | `should show shared tabs 4-6 (reuse)` | Inspection/Operation/Documents render |

### List, Filters, Auth (4 tests)
| # | Test | Expected |
|---|------|----------|
| 13 | `should filter by ODA project type` | Dropdown filters correctly |
| 14 | `should show [Xem] action (v1.1)` | Eye icon → detail page |
| 15 | `BTC full access` | All CRUD succeed |
| 16 | `CDT edit other's → 403 toast` | API 403 → error toast (F16) |

## Files to Create
- `e2e/fixtures/page-objects/oda-project.po.ts`
- `e2e/oda-projects/crud-flow.spec.ts`
- `e2e/oda-projects/validation-negative.spec.ts`
- `e2e/oda-projects/oda-specific-fields.spec.ts`
- `e2e/oda-projects/authorization.spec.ts`
