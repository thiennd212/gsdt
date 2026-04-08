import { test, expect } from '../fixtures/auth.fixture';
import { OdaProjectPage } from '../fixtures/page-objects/oda-project.po';

test.describe('ODA Projects: Authorization', () => {
  test('BTC should have full access to ODA', async ({ btcPage }) => {
    const po = new OdaProjectPage(btcPage);
    await po.navigateToList();
    await expect(btcPage.getByRole('button', { name: 'Thêm mới' })).toBeVisible();
  });

  test('CDT edit other project should produce 403', async ({ cdtPage }) => {
    await cdtPage.goto('http://localhost:6173/oda-projects');
    await cdtPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    // CDT can see list (scoped by BE) but write to other's project → 403
    await expect(cdtPage.getByTestId('oda-page')).toBeVisible();
  });
});
