# Phase 01 — Setup & Auth Fixtures

**Status:** Complete | **Effort:** 1.5d | **Tests:** 3
**Depends on:** Phase 00
**Red-team:** Addresses F1 (OIDC/sessionStorage), F2 (storageState gap), F5 (data isolation), F6 (existing PW config), F11 (service deps), F13 (Ant helpers)

## Overview
Setup Playwright infrastructure: config, auth fixtures using sessionStorage injection (NOT storageState), Ant Design interaction helpers, test data isolation, Docker Compose for test DB + MinIO.

## Auth Strategy (F1/F2 fix)

GSDT uses `oidc-client-ts` with `WebStorageStateStore({ store: sessionStorage })`. Playwright's `storageState` only captures cookies + localStorage — sessionStorage is lost.

**Solution:** Inject tokens directly into sessionStorage via `page.evaluate()`.

### Option A: BE test-token endpoint (preferred)
Add `GET /api/v1/test/token?role=BTC` that returns a valid OIDC user object (only in Testing env). Auth fixture calls this, then injects into sessionStorage.

### Option B: Reuse TestAuthHandler headers
Set `X-Test-UserId` + `X-Test-Roles` headers on every API call. Requires TestAuthHandler enabled in Development mode.

### Auth fixture pseudocode
```typescript
import { test as base, Page } from '@playwright/test';

type Role = 'BTC' | 'CQCQ' | 'CDT';

async function loginAs(page: Page, role: Role) {
  // 1. Get test token from BE
  const res = await page.request.get(`http://localhost:6001/api/v1/test/token?role=${role}`);
  const token = await res.json();

  // 2. Navigate to app origin (required for sessionStorage)
  await page.goto('http://localhost:6173');

  // 3. Inject into sessionStorage (matching oidc-client-ts key format)
  await page.evaluate((t) => {
    sessionStorage.setItem('oidc.user:http://localhost:6002:gsdt-spa-dev', JSON.stringify(t));
  }, token.data);

  // 4. Reload — AuthProvider picks up stored user
  await page.reload();
  await page.waitForSelector('[data-testid="sidebar-menu"]', { timeout: 10000 });
}

// Export as Playwright fixture
export const test = base.extend<{ loginBTC: Page; loginCQCQ: Page; loginCDT: Page }>({
  loginBTC: async ({ page }, use) => { await loginAs(page, 'BTC'); await use(page); },
  loginCQCQ: async ({ page }, use) => { await loginAs(page, 'CQCQ'); await use(page); },
  loginCDT: async ({ page }, use) => { await loginAs(page, 'CDT'); await use(page); },
});
```

## Test Data Isolation (F5 fix)

```typescript
// e2e/fixtures/test-data.fixture.ts
const RUN_ID = `e2e-${Date.now()}`;

export function testProjectCode(suffix: string) {
  return `${RUN_ID}-${suffix}`;
}

export async function cleanupTestData(request: APIRequestContext) {
  // DELETE all entities with e2e- prefix via dedicated cleanup endpoint
  await request.delete('http://localhost:6001/api/v1/test/cleanup', {
    data: { prefix: RUN_ID },
  });
}
```

## Ant Design Helpers (F13 fix)

```typescript
// e2e/fixtures/ant-helpers.ts — handle Ant's complex DOM

/** Click open an Ant Select, search, and pick option */
async function antSelect(page: Page, testId: string, optionText: string) {
  await page.getByTestId(testId).click();
  await page.waitForSelector('.ant-select-dropdown', { state: 'visible' });
  await page.locator('.ant-select-item-option').filter({ hasText: optionText }).click();
  await page.waitForSelector('.ant-select-dropdown', { state: 'hidden' });
}

/** Pick a date in Ant DatePicker */
async function antDatePick(page: Page, testId: string, date: string) {
  await page.getByTestId(testId).click();
  // clear + type formatted date directly
  await page.getByTestId(testId).locator('input').fill(date);
  await page.keyboard.press('Enter');
}

/** Confirm an Ant Popconfirm */
async function antConfirmPopup(page: Page) {
  await page.locator('.ant-popconfirm-buttons .ant-btn-primary').click();
}
```

## Playwright Config Update (F6 fix)

```typescript
// Preserve existing 7 projects, add new gsdt-e2e project
{
  name: 'gsdt-e2e',
  testDir: './e2e',
  testMatch: /\.(spec|test)\.(ts|tsx)$/,
  use: { ...devices['Desktop Chrome'] },
}
```

Update `baseURL: 'http://localhost:6173'`.

## Docker Compose for Tests (F11/F18 fix)

Create `infra/docker/docker-compose.test.yml`:
- SQL Server 2022 on port 1434 (separate from dev :1433)
- MinIO on port 9001 (for file upload tests)
- DB name: `GSDT_Test`

## Global Setup

```typescript
// e2e/global-setup.ts
// 1. Health-check: GET http://localhost:6001/health/ready
// 2. Health-check: GET http://localhost:6002/.well-known/openid-configuration
// 3. Health-check: GET http://localhost:6173 (FE loads)
// 4. Seed test users (BTC/CQCQ/CDT) if not exist
```

## Animation Disable (F13)

Inject CSS in every test context:
```typescript
// In test fixture or global setup
await page.addStyleTag({ content: '* { transition: none !important; animation: none !important; }' });
```

## Test Cases (smoke auth)

| # | Test | Expected |
|---|------|----------|
| 1 | `should login as BTC via sessionStorage injection` | Dashboard + admin menu visible |
| 2 | `should login as CQCQ` | Dashboard visible, admin restricted |
| 3 | `should login as CDT` | Dashboard + project menu visible |

## Files to Create
- `e2e/global-setup.ts`
- `e2e/global-teardown.ts`
- `e2e/fixtures/auth.fixture.ts`
- `e2e/fixtures/test-data.fixture.ts`
- `e2e/fixtures/ant-helpers.ts`
- `e2e/fixtures/page-objects/common.po.ts`
- `e2e/auth/login-roles.spec.ts`
- `infra/docker/docker-compose.test.yml` (optional — can use dev DB initially)

## BE Prerequisite
Add `TestTokenController` (Testing env only): `GET /api/v1/test/token?role={role}` — returns OIDC-compatible user object for Playwright auth injection.

## Todo
- [ ] Update playwright.config.ts (baseURL, add gsdt-e2e project, preserve existing)
- [ ] Create auth.fixture.ts with sessionStorage injection
- [ ] Create test-data.fixture.ts with run-ID prefix + cleanup
- [ ] Create ant-helpers.ts (antSelect, antDatePick, antConfirmPopup)
- [ ] Create common.po.ts (table helpers, form helpers, toast assertions)
- [ ] Create global-setup.ts (health checks + seed)
- [ ] Create global-teardown.ts (cleanup)
- [ ] Add BE TestTokenController (Testing env only)
- [ ] Add animation-disable CSS injection
- [ ] Write 3 auth smoke tests
- [ ] Verify tests pass with all 3 services running
