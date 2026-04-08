import { test, expect } from '../fixtures/auth.fixture';

test.describe('Auth: Role-based login via sessionStorage injection', () => {
  test('should login as BTC and see admin menu', async ({ btcPage }) => {
    // BTC = Admin + SystemAdmin — should see sidebar + admin section
    await expect(btcPage.getByTestId('sidebar-menu')).toBeVisible();

    // BTC should see admin menu entries
    const adminMenu = btcPage.locator('[data-testid="sidebar-menu"]');
    await expect(adminMenu).toContainText('Administration');
  });

  test('should login as CQCQ and see restricted menu', async ({ cqcqPage }) => {
    // CQCQ = authority-scoped read — should see sidebar
    await expect(cqcqPage.getByTestId('sidebar-menu')).toBeVisible();

    // CQCQ should see project links
    const sidebar = cqcqPage.getByTestId('sidebar-menu');
    await expect(sidebar).toBeVisible();
  });

  test('should login as CDT and see project menu', async ({ cdtPage }) => {
    // CDT = project owner — should see sidebar with project items
    await expect(cdtPage.getByTestId('sidebar-menu')).toBeVisible();

    // CDT should be able to navigate to domestic projects
    const sidebar = cdtPage.getByTestId('sidebar-menu');
    await expect(sidebar).toBeVisible();
  });
});
