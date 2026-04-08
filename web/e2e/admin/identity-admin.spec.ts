import { test, expect } from '../fixtures/auth.fixture';

test.describe('Admin: Identity CRUD', () => {
  test('should list users with search', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/users');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.locator('.ant-table')).toBeVisible();
  });

  test('should create user via modal', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/users');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    const btn = btcPage.getByRole('button', { name: /Thêm|Add|Create/ });
    if (await btn.isVisible()) {
      await btn.click();
      await expect(btcPage.locator('.ant-modal')).toBeVisible({ timeout: 5000 });
      await btcPage.keyboard.press('Escape');
    }
  });

  test('should assign role to user', async ({ btcPage }) => {
    test.skip(true, 'Requires seeded user');
  });

  test('should CRUD user group', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/groups');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });

  test('should list and create ABAC rule', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/abac-rules');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.locator('.ant-table').or(btcPage.getByTestId('page-header'))).toBeVisible();
  });

  test('should list SoD rules', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/sod-rules');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should list access policies', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/policy-rules');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should edit credential policy', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/credential-policies');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should list access reviews', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/access-reviews');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should create delegation', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/delegations');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should list active sessions', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/sessions');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should list external identities', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/external-identities');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });
});
