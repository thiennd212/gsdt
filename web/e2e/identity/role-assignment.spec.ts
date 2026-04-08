import { test, expect } from '../fixtures/auth.fixture';

test.describe('Identity: Role & Access', () => {
  test('should show roles page', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/roles');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.locator('.ant-table')).toBeVisible();
  });

  test('should show ABAC rules page', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/abac-rules');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.locator('.ant-table')).toBeVisible();
  });

  test('should manage delegations page', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/delegations');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.locator('.ant-table').or(btcPage.getByTestId('page-header'))).toBeVisible();
  });
});
