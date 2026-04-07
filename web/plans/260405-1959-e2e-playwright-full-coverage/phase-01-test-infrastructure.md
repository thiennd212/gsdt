# Phase 1: Test Infrastructure (POM + Fixtures)

## Overview
- **Priority:** P1 — foundation for all other phases
- **Effort:** 2h
- **Status:** Done
- **Dependencies:** None (standalone)
- **Blocks:** Phases 2-6

## Context
- Existing: `e2e/helpers/auth-helper.ts` (ROPC token), `playwright.config.ts` (5 projects)
- Missing: Page Object Model, test data factory, reusable fixtures, shared helpers
- Pattern: browser-ui.spec.ts uses inline `loginViaOidc()` — needs extraction to POM

## Key Insights
- Ant Design uses `.ant-*` class selectors — fragile across versions. Prefer `data-testid`, `role`, or text selectors
- OIDC login via Docker uses port 3000 (web) + 5000 (auth); Vite dev uses 5173
- Serial mode required for browser tests sharing auth state
- Test data must be created via API (not DB) to respect business rules

## Files to Create

| File | Purpose | LOC est |
|------|---------|---------|
| `e2e/pages/login-page.ts` | OIDC login POM | ~40 |
| `e2e/pages/admin-layout-page.ts` | Sidebar nav, topbar, breadcrumb | ~60 |
| `e2e/pages/data-table-page.ts` | Reusable table: filter, sort, paginate, row select | ~80 |
| `e2e/pages/form-modal-page.ts` | Reusable modal/drawer form: fill, submit, validate | ~50 |
| `e2e/pages/public-form-page.ts` | Public form: steps, fields, upload, submit | ~50 |
| `e2e/fixtures/auth-fixture.ts` | Playwright fixture: authenticated page + API token | ~60 |
| `e2e/fixtures/test-data-factory.ts` | API-driven seed + cleanup for test entities | ~80 |
| `e2e/helpers/navigation-helper.ts` | goto + waitForContent + assertNoJsErrors | ~40 |
| `e2e/helpers/form-helper.ts` | Fill form fields by label, handle Select/DatePicker | ~50 |
| `e2e/helpers/table-helper.ts` | Table assertions: row count, cell text, sort order | ~40 |

## Architecture

```
Playwright base test
  |-- extended by auth-fixture.ts
  |     |-- provides: authedPage (browser), apiToken (string), apiContext
  |     |-- beforeAll: OIDC login via LoginPage POM → stores state
  |     |-- provides: testData (TestDataFactory instance, auto-cleanup)
  |
  v
Page Objects (pages/*.ts)
  |-- LoginPage: navigate, fillCredentials, submit, waitForRedirect
  |-- AdminLayoutPage: navigateToMenu(label), getCurrentRoute, getBreadcrumb
  |-- DataTablePage: getRowCount, getColumnValues, filterBy, sortBy, paginate
  |-- FormModalPage: fill(fieldMap), submit, getValidationErrors, close
  |-- PublicFormPage: fillStep, nextStep, uploadFile, submitForm
```

## Implementation Steps

### 1. Create POM base — `e2e/pages/login-page.ts`
```typescript
// Encapsulates OIDC login flow from browser-ui.spec.ts
export class LoginPage {
  constructor(private page: Page) {}
  
  async goto() { /* navigate to WEB_URL */ }
  async login(email: string, password: string) { /* fill + submit OIDC form */ }
  async waitForAuthenticated() { /* wait for redirect back to app */ }
}
```

### 2. Create `e2e/pages/admin-layout-page.ts`
```typescript
export class AdminLayoutPage {
  constructor(private page: Page) {}
  
  async navigateToMenu(menuLabel: string) { /* click sidebar item */ }
  async navigateToRoute(path: string) { /* direct goto + wait */ }
  async getCurrentBreadcrumb(): Promise<string[]> { /* read breadcrumb */ }
  async isSidebarVisible(): Promise<boolean> { /* check sidebar */ }
  async getPageTitle(): Promise<string> { /* read page header */ }
}
```

### 3. Create `e2e/pages/data-table-page.ts`
```typescript
export class DataTablePage {
  constructor(private page: Page, private tableSelector?: string) {}
  
  async getRowCount(): Promise<number> {}
  async getCellText(row: number, col: number): Promise<string> {}
  async filterByColumn(column: string, value: string) {}
  async sortByColumn(column: string) {}
  async goToPage(pageNum: number) {}
  async getSelectedRowCount(): Promise<number> {}
  async selectRow(index: number) {}
  async selectAllRows() {}
}
```

### 4. Create `e2e/pages/form-modal-page.ts`
```typescript
export class FormModalPage {
  constructor(private page: Page) {}
  
  async waitForOpen() { /* .ant-modal or .ant-drawer visible */ }
  async fillField(label: string, value: string) {}
  async fillSelect(label: string, option: string) {}
  async fillDatePicker(label: string, date: string) {}
  async submit() {}
  async cancel() {}
  async getValidationErrors(): Promise<string[]> {}
  async isOpen(): Promise<boolean> {}
}
```

### 5. Create `e2e/pages/public-form-page.ts`
```typescript
export class PublicFormPage {
  constructor(private page: Page) {}
  
  async goto(formCode: string) { /* /public/forms/{code} */ }
  async fillTextField(label: string, value: string) {}
  async fillSelectField(label: string, option: string) {}
  async uploadFile(label: string, filePath: string) {}
  async nextStep() {}
  async previousStep() {}
  async getCurrentStep(): Promise<number> {}
  async submit() {}
  async getConfirmationMessage(): Promise<string> {}
}
```

### 6. Create `e2e/fixtures/auth-fixture.ts`
```typescript
import { test as base, Page } from '@playwright/test';
import { LoginPage } from '../pages/login-page';
import { TestDataFactory } from './test-data-factory';

type AuthFixtures = {
  authedPage: Page;
  apiToken: string;
  testData: TestDataFactory;
};

export const test = base.extend<{}, AuthFixtures>({
  authedPage: [async ({ browser }, use) => {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('admin@dev.local', 'DevAdmin@12345');
    await loginPage.waitForAuthenticated();
    await use(page);
    await ctx.close();
  }, { scope: 'worker' }],
  
  apiToken: [async ({ playwright }, use) => {
    // ROPC token via existing auth-helper pattern
    const ctx = await playwright.request.newContext();
    const token = await getAccessToken(ctx);
    await use(token);
    await ctx.dispose();
  }, { scope: 'worker' }],
  
  testData: [async ({ apiToken, playwright }, use) => {
    const ctx = await playwright.request.newContext();
    const factory = new TestDataFactory(ctx, apiToken);
    await use(factory);
    await factory.cleanup(); // delete all created entities
    await ctx.dispose();
  }, { scope: 'worker' }],
});
```

### 7. Create `e2e/fixtures/test-data-factory.ts`
```typescript
export class TestDataFactory {
  private createdEntities: Array<{ type: string; id: string }> = [];
  
  constructor(private request: APIRequestContext, private token: string) {}
  
  async createCase(overrides?: Partial<CaseData>): Promise<{ id: string }> {}
  async createFormTemplate(overrides?: Partial<FormData>): Promise<{ id: string }> {}
  async createUser(overrides?: Partial<UserData>): Promise<{ id: string }> {}
  async createOrgUnit(overrides?: Partial<OrgData>): Promise<{ id: string }> {}
  
  async cleanup() {
    // Reverse-order deletion to respect FK constraints
    for (const entity of this.createdEntities.reverse()) {
      await this.delete(entity.type, entity.id);
    }
  }
}
```

### 8. Create `e2e/helpers/navigation-helper.ts`
```typescript
export async function navigateAndWait(page: Page, path: string) {
  const errors: string[] = [];
  page.on('pageerror', (err) => errors.push(err.message));
  
  await page.goto(path);
  await page.waitForLoadState('domcontentloaded');
  await page.locator('.ant-layout-content, main').first().waitFor({ timeout: 10_000 });
  
  return { errors };
}

export async function assertNoJsErrors(page: Page, during: () => Promise<void>) {
  const errors: string[] = [];
  page.on('pageerror', (err) => errors.push(err.message));
  await during();
  expect(errors).toHaveLength(0);
}
```

### 9. Create `e2e/helpers/form-helper.ts`
```typescript
export async function fillFormField(page: Page, label: string, value: string) {
  const field = page.getByLabel(label);
  await field.fill(value);
}

export async function selectOption(page: Page, label: string, option: string) {
  await page.getByLabel(label).click();
  await page.getByTitle(option).click();
}

export async function fillDatePicker(page: Page, label: string, date: string) {
  await page.getByLabel(label).click();
  await page.locator('.ant-picker-input input').fill(date);
  await page.keyboard.press('Enter');
}
```

### 10. Create `e2e/helpers/table-helper.ts`
```typescript
export async function getTableRowCount(page: Page, selector?: string): Promise<number> {
  const table = page.locator(selector ?? '.ant-table-tbody');
  return table.locator('tr.ant-table-row').count();
}

export async function getColumnValues(page: Page, colIndex: number): Promise<string[]> {
  const cells = page.locator(`.ant-table-tbody tr.ant-table-row td:nth-child(${colIndex + 1})`);
  return cells.allTextContents();
}
```

### 11. Update `playwright.config.ts` — add new projects
Add two new project entries:
```typescript
{
  name: 'e2e-browser',
  testMatch: /browser-ui\//,        // new browser-ui/ subfolder
  use: { ...devices['Desktop Chrome'] },
},
{
  name: 'e2e-perf',
  testMatch: /performance-baselines/,
  use: { ...devices['Desktop Chrome'] },
},
```

## Test Matrix

| Test | Scenario | Expected |
|------|----------|----------|
| LoginPage.login() | Valid credentials | Redirect to app, page visible |
| LoginPage.login() | Invalid credentials | Error message visible |
| AdminLayoutPage.navigateToMenu() | Click "Users" in sidebar | URL contains /admin/users |
| DataTablePage.getRowCount() | Table with data | Count > 0 |
| DataTablePage.filterByColumn() | Filter by text | Rows filtered |
| FormModalPage.fillField() | Fill text input | Value set |
| FormModalPage.getValidationErrors() | Submit empty required form | Error messages returned |
| TestDataFactory.createCase() | Create via API | Returns { id } |
| TestDataFactory.cleanup() | After test | All created entities deleted |
| auth-fixture authedPage | Use in test | Page is authenticated, no login prompt |

## Success Criteria
- [x] `LoginPage` encapsulates OIDC flow, works with Docker (port 3000) and Vite (port 5173)
- [x] `auth-fixture.ts` provides `authedPage` to downstream tests — verified with single smoke test
- [x] `TestDataFactory` creates + cleans up at least: case, user, form template
- [x] `DataTablePage` works with Ant Design table — verified on /admin/users
- [x] All helper functions exported and importable
- [x] No changes to existing spec files

## Risk Assessment
| Risk | Mitigation |
|------|------------|
| Ant Design selector changes across versions | Use `getByRole`/`getByText` as primary, `.ant-*` as fallback |
| Worker-scoped fixture may fail if auth server slow | 120s timeout, retry in fixture setup |
| TestDataFactory cleanup order matters (FK) | Track creation order, reverse on cleanup |
