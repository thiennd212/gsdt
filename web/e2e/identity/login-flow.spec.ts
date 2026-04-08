import { test, expect } from '../fixtures/auth.fixture';

test.describe('Identity: Login Flow', () => {
  test('should redirect unauthenticated to login', async ({ page }) => {
    // Visit app without auth — should redirect to OIDC login
    await page.goto('http://localhost:6173/');
    // Either redirected to auth server or shows login prompt
    const url = page.url();
    const isRedirected = url.includes('localhost:6002') || url.includes('/login') || url.includes('/callback');
    const hasLoginContent = await page.locator('text=Sign in').or(page.locator('text=Login')).isVisible().catch(() => false);
    expect(isRedirected || hasLoginContent).toBeTruthy();
  });

  test('should login with valid credentials (via fixture)', async ({ btcPage }) => {
    // btcPage is pre-authenticated via sessionStorage injection
    await expect(btcPage.getByTestId('sidebar-menu')).toBeVisible();
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });

  test('should show forbidden page for unauthorized route', async ({ cdtPage }) => {
    // CDT accessing admin route should see 403
    await cdtPage.goto('http://localhost:6173/admin/users');
    const forbidden = cdtPage.locator('text=403').or(cdtPage.locator('text=Forbidden')).or(cdtPage.locator('text=không có quyền'));
    const hasForbidden = await forbidden.isVisible().catch(() => false);
    expect(hasForbidden).toBeTruthy();
  });
});
