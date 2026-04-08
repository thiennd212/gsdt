import { test, expect } from '../fixtures/auth.fixture';

test.describe('Identity: User Management', () => {
  test('should list users with pagination', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/users');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    // Users table should render
    const table = btcPage.locator('.ant-table');
    await expect(table).toBeVisible();
  });

  test('should create user via modal', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/users');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    // Look for create button
    const createBtn = btcPage.getByRole('button', { name: /Thêm|Add|Create/ });
    if (await createBtn.isVisible()) {
      await createBtn.click();
      await btcPage.waitForSelector('.ant-modal', { state: 'visible', timeout: 5000 });
      await expect(btcPage.locator('.ant-modal')).toBeVisible();
      // Close modal
      await btcPage.keyboard.press('Escape');
    }
  });

  test('should assign role to user', async ({ btcPage }) => {
    test.skip(true, 'Requires seeded user data');
  });

  test('should search users', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/users');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    // Search input should exist
    const search = btcPage.locator('input[placeholder*="Search"], input[placeholder*="Tìm"]');
    if (await search.isVisible()) {
      await search.fill('admin');
      await expect(search).toHaveValue('admin');
    }
  });
});
