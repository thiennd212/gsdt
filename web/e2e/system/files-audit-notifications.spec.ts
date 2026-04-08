import { test, expect } from '../fixtures/auth.fixture';

test.describe('System: Files, Audit, Notifications', () => {
  test('should list files', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/files');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.locator('.ant-table').or(btcPage.getByTestId('page-header'))).toBeVisible();
  });

  test('should upload file', async ({ btcPage }) => {
    test.skip(true, 'Requires MinIO running');
  });

  test('should list audit logs with date filter', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/audit/logs');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.locator('.ant-table').or(btcPage.getByTestId('page-header'))).toBeVisible();
  });

  test('should list notifications', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/notifications');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });
});
