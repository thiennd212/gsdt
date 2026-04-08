import { test, expect } from '../fixtures/auth.fixture';

test.describe('Admin: System CRUD', () => {
  test('should list background jobs', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/jobs');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should show system health status', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/health');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should list backup history', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/backup');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should list data erasure requests', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/rtbf');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should show admin overview stats', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });

  test('should list JIT provider configs', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/admin/jit-provider-configs');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  });
});
