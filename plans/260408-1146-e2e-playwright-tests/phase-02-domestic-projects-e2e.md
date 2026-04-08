# Phase 02 — Domestic Projects E2E

**Status:** Complete | **Effort:** 2d | **Tests:** 20 | **Depends on:** P00, P01
**Red-team:** Addresses F7 (negative tests), F9 (concurrency), F10 (tab persistence), F16 (BE-enforced auth)

## Test Cases

### CRUD Flow — Happy Path (5 tests)
| # | Test | Steps | Expected |
|---|------|-------|----------|
| 1 | `should create domestic project via Tab 1` | Fill required fields → Lưu thông tin | Created, redirects to edit, Tabs 2-6 enabled |
| 2 | `should list created project` | Navigate to /domestic-projects | Project visible with correct columns |
| 3 | `should edit project Tab 1` | Click [Sửa] → change name → Lưu | Updated, success toast |
| 4 | `should view detail (readonly)` | Click [Xem] → verify disabled | All inputs disabled, Chỉnh sửa button |
| 5 | `should soft-delete project` | Click [Xóa] → confirm → OK | Removed from list, success toast |

### CRUD Flow — Negative (5 tests, F7 fix)
| # | Test | Steps | Expected |
|---|------|-------|----------|
| 6 | `should show validation errors on empty submit` | Click Lưu with empty form | Inline errors on required fields |
| 7 | `should reject duplicate project code` | Create with existing code | API 409 → error toast "đã tồn tại" |
| 8 | `should handle concurrent edit (RowVersion)` | Open 2 tabs → save both | Second save → 409 toast "dữ liệu đã bị thay đổi" (F9) |
| 9 | `should preserve Tab 2 state after Tab 1 save failure` | Edit Tab 2 → switch Tab 1 → fail save → back Tab 2 | Tab 2 form data preserved (F10) |
| 10 | `should enable Tabs 2-6 after create redirect` | Create on Tab 1 → redirects to edit | Tabs 2-6 no longer disabled (F10) |

### Tab Navigation & Sub-entities (4 tests)
| # | Test | Expected |
|---|------|----------|
| 11 | `should show saved/unsaved badges` | Tab 1 green check after save, Tab 2 orange dot on edit |
| 12 | `should add/delete location (Tab 1 Zone 4)` | Province→District cascade, row in table, delete works |
| 13 | `should add investment decision (Tab 1 Zone 5)` | Form → Lưu thông tin → row in table |
| 14 | `should add bid package via popup (Tab 2)` | Modal with KHLCNT combobox (v1.1), save → row in table |

### Filters & Search (3 tests)
| # | Test | Expected |
|---|------|----------|
| 15 | `should filter by search text` | Type name → Tìm kiếm → filtered results |
| 16 | `should filter by dropdown` | Select CQ quản lý → Tìm kiếm → filtered |
| 17 | `should clear all filters` | Xóa bộ lọc → reset all, full list |

### Authorization — BE-enforced (3 tests, F16 fix)
| # | Test | Expected |
|---|------|----------|
| 18 | `BTC should CRUD all projects` | All operations succeed |
| 19 | `CQCQ edit → 403 error toast` | Click Sửa → save → API 403 → error toast (NOT hidden button) |
| 20 | `CDT should see own projects only` | List shows only own, other's → 403 on direct URL |

## Page Object
```typescript
// e2e/fixtures/page-objects/domestic-project.po.ts
class DomesticProjectPage {
  async navigateToList() { ... }
  async clickCreate() { ... }
  async fillTab1Required(data: { code, name, authority, sector, owner, group }) { ... }
  async saveTab1() { await page.getByTestId('domestic-btn-save-tab1').click(); }
  async switchTab(n: number) { ... }
  async addLocation(province, district, address) { ... }
  async addDecision(data) { ... }
  async addBidPackage(data) { ... }
  async getTableRowCount() { ... }
  async searchByText(query) { ... }
  async deleteProject(name) { ... }
  async verifyTabBadge(tabN, status: 'saved' | 'unsaved' | 'idle') { ... }
  async verifyValidationError(field, message) { ... }
  async verifyToast(type: 'success' | 'error', text) { ... }
}
```

## Files to Create
- `e2e/fixtures/page-objects/domestic-project.po.ts`
- `e2e/domestic-projects/crud-flow.spec.ts` (tests 1-5)
- `e2e/domestic-projects/validation-negative.spec.ts` (tests 6-10)
- `e2e/domestic-projects/tab-navigation.spec.ts` (tests 11-14)
- `e2e/domestic-projects/filters-search.spec.ts` (tests 15-17)
- `e2e/domestic-projects/authorization.spec.ts` (tests 18-20)

## Todo
- [ ] Create domestic-project.po.ts page object
- [ ] CRUD happy path (5 tests)
- [ ] Validation/negative tests (5 tests)
- [ ] Tab navigation + sub-entity tests (4 tests)
- [ ] Filter/search tests (3 tests)
- [ ] Authorization tests (3 tests)
- [ ] All 20 tests passing
