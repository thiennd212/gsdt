import { test, expect } from '../fixtures/auth.fixture';

test.describe('Admin: Content CRUD', () => {
  test('should list document templates', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/templates');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });

  test('should create template', async ({ btcPage }) => {
    test.skip(true, 'Requires template creation API');
  });

  test('should list notification templates', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/notification-templates');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should edit notification template', async ({ btcPage }) => {
    test.skip(true, 'Requires seeded template');
  });

  test('should list menu items', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/menus');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should create menu item', async ({ btcPage }) => {
    test.skip(true, 'Requires menu creation form');
  });
});
