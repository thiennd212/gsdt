import { test, expect } from '../fixtures/auth.fixture';

// Smoke tests for 20+ remaining admin modules — navigate + renders without error
test.describe('System: Remaining Modules Smoke', () => {
  const adminPages = [
    { name: 'groups', path: '/admin/groups' },
    { name: 'abac-rules', path: '/admin/abac-rules' },
    { name: 'sod-rules', path: '/admin/sod-rules' },
    { name: 'policy-rules', path: '/admin/policy-rules' },
    { name: 'credential-policies', path: '/admin/credential-policies' },
    { name: 'access-reviews', path: '/admin/access-reviews' },
    { name: 'delegations', path: '/admin/delegations' },
    { name: 'sessions', path: '/admin/sessions' },
    { name: 'external-identities', path: '/admin/external-identities' },
    { name: 'jit-provider-configs', path: '/admin/jit-provider-configs' },
  ];

  test('should render identity admin pages without error', async ({ btcPage }) => {
    for (const { name, path } of adminPages) {
      await btcPage.goto(`http://localhost:6173${path}`);
      const header = btcPage.getByTestId('page-header');
      await expect(header).toBeVisible({ timeout: 10000 });
    }
  });

  test('should render admin system pages without error', async ({ btcPage }) => {
    const systemPages = ['/admin/jobs', '/admin/health', '/admin/backup', '/admin/rtbf', '/admin'];
    for (const path of systemPages) {
      await btcPage.goto(`http://localhost:6173${path}`);
      await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    }
  });

  test('should render admin content pages without error', async ({ btcPage }) => {
    const contentPages = ['/admin/templates', '/admin/notification-templates', '/admin/menus'];
    for (const path of contentPages) {
      await btcPage.goto(`http://localhost:6173${path}`);
      await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    }
  });

  test('should render admin integration pages without error', async ({ btcPage }) => {
    const integPages = ['/admin/api-keys', '/admin/webhooks', '/admin/ai', '/admin/data-scopes'];
    for (const path of integPages) {
      await btcPage.goto(`http://localhost:6173${path}`);
      await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    }
  });

  test('should render user pages without error', async ({ btcPage }) => {
    const userPages = ['/profile', '/consent'];
    for (const path of userPages) {
      await btcPage.goto(`http://localhost:6173${path}`);
      await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    }
  });
});
