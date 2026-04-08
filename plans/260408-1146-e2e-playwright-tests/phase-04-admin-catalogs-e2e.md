# Phase 04 — Admin Catalogs E2E

**Status:** Complete | **Effort:** 0.5d | **Tests:** 10 | **Depends on:** P00, P01
**Red-team:** Addresses F7 (negative tests)

## Generic Catalogs (4 tests)
| # | Test | Expected |
|---|------|----------|
| 1 | `should navigate catalog index → generic catalog` | Cards visible, click navigates |
| 2 | `should create generic catalog item` | Modal → Code+Name → save → in table |
| 3 | `should edit catalog item` | Sửa → change name → save → updated |
| 4 | `should soft-delete catalog item` | Confirm → removed from active list |

## KHLCNT (3 tests)
| # | Test | Expected |
|---|------|----------|
| 5 | `should create KHLCNT with 4 fields` | NameVi, NameEn, NgàyKý, NguờiKý → STT auto |
| 6 | `should edit KHLCNT` | Pre-filled, date picker works |
| 7 | `should show KHLCNT in bid package combobox` | Create → open bid modal → visible in dropdown |

## Negative + Auth (3 tests, F7 fix)
| # | Test | Expected |
|---|------|----------|
| 8 | `should reject duplicate catalog code` | API 409 → error toast |
| 9 | `should restrict catalogs to BTC only` | CDT → /admin/catalogs → 403 page |
| 10 | `should show validation errors on empty create` | Empty Code/Name → inline errors |

## Files to Create
- `e2e/fixtures/page-objects/catalog.po.ts`
- `e2e/admin-catalogs/generic-catalog.spec.ts`
- `e2e/admin-catalogs/khlcnt.spec.ts`
- `e2e/admin-catalogs/catalog-negative-auth.spec.ts`
