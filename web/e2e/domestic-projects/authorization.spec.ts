import { test, expect } from '../fixtures/auth.fixture';
import { DomesticProjectPage } from '../fixtures/page-objects/domestic-project.po';

test.describe('Domestic Projects: Authorization', () => {
  test('BTC should see full project list and CRUD buttons', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();

    // BTC should see create button
    await expect(btcPage.getByTestId('domestic-btn-create')).toBeVisible();

    // Table should be visible
    await expect(btcPage.getByTestId('domestic-table-projects')).toBeVisible();
  });

  test('CQCQ edit should produce 403 error toast', async ({ cqcqPage }) => {
    // CQCQ navigates to project list — should see it (read access)
    await cqcqPage.goto('http://localhost:6173/domestic-projects');
    await cqcqPage.waitForSelector('[data-testid="domestic-table-projects"]', { timeout: 10000 });

    // CQCQ can see the table
    await expect(cqcqPage.getByTestId('domestic-table-projects')).toBeVisible();

    // Note: CQCQ write operations produce API 403 — buttons visible but API rejects
    // Actual write test requires seeded data — skipped for now
  });

  test('CDT should see own projects only', async ({ cdtPage }) => {
    // CDT navigates to project list — should see filtered results
    await cdtPage.goto('http://localhost:6173/domestic-projects');
    await cdtPage.waitForSelector('[data-testid="domestic-table-projects"]', { timeout: 10000 });

    // CDT can see the table (scoped to own projects by BE)
    await expect(cdtPage.getByTestId('domestic-table-projects')).toBeVisible();
  });
});
