import { test, expect } from '../fixtures/auth.fixture';

test.describe('Smoke: Performance Baselines', () => {
  test('project list page load < 3s', async ({ btcPage }) => {
    const start = Date.now();
    await btcPage.goto('http://localhost:6173/domestic-projects');
    await btcPage.waitForSelector('[data-testid="domestic-table-projects"]', { timeout: 10000 });
    const duration = Date.now() - start;
    expect(duration).toBeLessThan(3000);
  });

  test('project form page load < 5s', async ({ btcPage }) => {
    const start = Date.now();
    await btcPage.goto('http://localhost:6173/domestic-projects/new');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    const duration = Date.now() - start;
    expect(duration).toBeLessThan(5000);
  });
});
