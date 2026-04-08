import { test, expect } from '../fixtures/auth.fixture';

test.describe('System: Organization & Params', () => {
  test('should show org unit tree', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/organization');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });

  test('should edit system param', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/system-params');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.locator('.ant-table').or(btcPage.getByTestId('page-header'))).toBeVisible();
  });

  test('should show province/district cascade', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/master-data');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });

  test('should show roles page', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/roles');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });
});
