import { test, expect } from '../fixtures/auth.fixture';
import { CatalogPage } from '../fixtures/page-objects/catalog.po';
import { testCode } from '../fixtures/test-data.fixture';

test.describe('Admin Catalogs: Negative & Auth', () => {
  test('should reject duplicate catalog code', async ({ btcPage }) => {
    const po = new CatalogPage(btcPage);
    const code = testCode('DUP-CAT');
    await po.navigateToGeneric('banks');

    // Create first
    await po.clickCreate();
    await po.fillGenericForm(code, 'Dup Test 1');
    await po.submitModal();
    await po.waitForSuccess();

    // Try duplicate
    await po.clickCreate();
    await po.fillGenericForm(code, 'Dup Test 2');
    await po.submitModal();
    await po.waitForError();
  });

  test('should restrict catalog admin to BTC only', async ({ cdtPage }) => {
    // CDT navigating to admin catalogs should see 403
    await cdtPage.goto('http://localhost:6173/admin/catalogs');
    // AdminGuard checks for Admin/SystemAdmin roles
    // CDT doesn't have these — should see ForbiddenPage
    const forbidden = cdtPage.locator('text=403').or(cdtPage.locator('text=Forbidden'));
    const pageHeader = cdtPage.getByTestId('page-header');
    // Either forbidden page or no access
    const hasForbidden = await forbidden.isVisible().catch(() => false);
    const hasHeader = await pageHeader.isVisible().catch(() => false);
    // CDT should NOT see catalog admin content
    expect(hasForbidden || !hasHeader).toBeTruthy();
  });

  test('should show validation errors on empty create', async ({ btcPage }) => {
    const po = new CatalogPage(btcPage);
    await po.navigateToGeneric('document-types');
    await po.clickCreate();
    // Submit empty form
    await po.submitModal();
    // Should show validation errors
    const errors = btcPage.locator('.ant-form-item-explain-error');
    await expect(errors.first()).toBeVisible({ timeout: 3000 });
  });
});
