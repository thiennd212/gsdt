import { test, expect } from '../fixtures/auth.fixture';

test.describe('Admin: Integration CRUD', () => {
  test('should list and create API key', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/api-keys');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });

  test('should revoke API key', async ({ btcPage }) => {
    test.skip(true, 'Requires seeded API key');
  });

  test('should list webhook deliveries', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/webhooks');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should show AI admin config', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/ai');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should list data scopes', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/data-scopes');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should create data scope', async ({ btcPage }) => {
    test.skip(true, 'Requires data scope creation form');
  });
});
