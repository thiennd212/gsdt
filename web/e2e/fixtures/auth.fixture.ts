import { test as base, type Page } from '@playwright/test';

const BASE_URL = 'http://localhost:6173';
const API_URL = 'http://localhost:6001';
const OIDC_KEY = 'oidc.user:http://localhost:6002:gsdt-spa-dev';

export type Role = 'BTC' | 'CQCQ' | 'CDT';

// Fetch test token from BE TestTokenController and inject into sessionStorage.
// This bypasses the real OIDC flow — only works when BE is in Development/Testing env.
async function loginAs(page: Page, role: Role): Promise<void> {
  // 1. Get fake OIDC token from BE
  const res = await page.request.get(`${API_URL}/api/v1/test/token?role=${role}`);
  const json = await res.json();
  const token = json.data;

  // 2. Navigate to app origin (required to set sessionStorage on correct domain)
  await page.goto(BASE_URL);

  // 3. Inject token into sessionStorage (oidc-client-ts reads from here)
  await page.evaluate(
    ({ key, value }) => {
      sessionStorage.setItem(key, JSON.stringify(value));
    },
    { key: OIDC_KEY, value: token },
  );

  // 4. Reload — AuthProvider picks up stored user
  await page.reload();

  // 5. Wait for sidebar to confirm auth succeeded
  await page.waitForSelector('[data-testid="sidebar-menu"]', { timeout: 15000 });

  // 6. Disable animations for stability
  await page.addStyleTag({
    content: '*, *::before, *::after { transition: none !important; animation: none !important; }',
  });
}

// Playwright test fixture with pre-authenticated pages per role
type AuthFixtures = {
  btcPage: Page;
  cqcqPage: Page;
  cdtPage: Page;
  loginAsRole: (page: Page, role: Role) => Promise<void>;
};

export const test = base.extend<AuthFixtures>({
  btcPage: async ({ page }, use) => {
    await loginAs(page, 'BTC');
    await use(page);
  },
  cqcqPage: async ({ browser }, use) => {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    await loginAs(page, 'CQCQ');
    await use(page);
    await ctx.close();
  },
  cdtPage: async ({ browser }, use) => {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    await loginAs(page, 'CDT');
    await use(page);
    await ctx.close();
  },
  loginAsRole: async ({}, use) => {
    await use(loginAs);
  },
});

export { expect } from '@playwright/test';
