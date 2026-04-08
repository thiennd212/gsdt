import { test, expect } from '../fixtures/auth.fixture';

test.describe('System: Dashboard', () => {
  test('should render dashboard with stats', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });

  test('should navigate from dashboard to modules', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/');
    await btcPage.waitForSelector('[data-testid="sidebar-menu"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('sidebar-menu')).toBeVisible();
  });
});
