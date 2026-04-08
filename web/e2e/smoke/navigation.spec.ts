import { test, expect } from '../fixtures/auth.fixture';

test.describe('Smoke: Navigation', () => {
  test('should access all sidebar links without error', async ({ btcPage }) => {
    const routes = ['/domestic-projects', '/oda-projects', '/files', '/notifications', '/roles', '/audit/logs'];
    for (const route of routes) {
      await btcPage.goto(`http://localhost:6173${route}`);
      // No 500 error page — page header visible
      await expect(btcPage.getByTestId('page-header')).toBeVisible({ timeout: 10000 });
    }
  });

  test('should show breadcrumb on project pages', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/domestic-projects');
    await btcPage.waitForSelector('[data-testid="breadcrumb"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('breadcrumb')).toBeVisible();
  });

  test('should show 404 for invalid route', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/nonexistent-page-xyz');
    const notFound = btcPage.locator('text=404').or(btcPage.locator('text=Not Found'));
    await expect(notFound).toBeVisible({ timeout: 10000 });
  });
});
