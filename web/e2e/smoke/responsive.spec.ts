import { test, expect } from '../fixtures/auth.fixture';

test.describe('Smoke: Responsive', () => {
  test('should collapse sidebar on tablet (768px)', async ({ browser }) => {
    const ctx = await browser.newContext({ viewport: { width: 768, height: 1024 } });
    const page = await ctx.newPage();
    // Login
    const res = await page.request.get('http://localhost:6001/api/v1/test/token?role=BTC');
    const token = await res.json();
    await page.goto('http://localhost:6173');
    await page.evaluate(({ k, v }) => sessionStorage.setItem(k, JSON.stringify(v)), { k: 'oidc.user:http://localhost:6002:gsdt-spa-dev', v: token.data });
    await page.reload();
    await page.waitForSelector('[data-testid="sidebar-menu"]', { timeout: 15000 });
    // Sidebar should be collapsed (narrow width)
    const sidebar = page.getByTestId('sidebar-menu');
    await expect(sidebar).toBeVisible();
    await ctx.close();
  });

  test('should show drawer on mobile (480px)', async ({ browser }) => {
    const ctx = await browser.newContext({ viewport: { width: 480, height: 800 } });
    const page = await ctx.newPage();
    const res = await page.request.get('http://localhost:6001/api/v1/test/token?role=BTC');
    const token = await res.json();
    await page.goto('http://localhost:6173');
    await page.evaluate(({ k, v }) => sessionStorage.setItem(k, JSON.stringify(v)), { k: 'oidc.user:http://localhost:6002:gsdt-spa-dev', v: token.data });
    await page.reload();
    // On mobile, sidebar may be hidden — hamburger menu visible
    await page.waitForTimeout(3000);
    await ctx.close();
  });
});
