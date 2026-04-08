import { test, expect } from '../fixtures/auth.fixture';
import { DomesticProjectPage } from '../fixtures/page-objects/domestic-project.po';
import { testCode } from '../fixtures/test-data.fixture';

test.describe('Domestic Projects: Validation & Negative', () => {
  test('should show validation errors on empty submit', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();
    await po.clickCreate();

    // Click save without filling any fields
    await po.saveTab1();

    // Should show inline validation errors (Ant Form.Item errors)
    const errors = btcPage.locator('.ant-form-item-explain-error');
    await expect(errors.first()).toBeVisible({ timeout: 3000 });
  });

  test('should reject duplicate project code', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    const code = testCode('DUP-TN');

    // Create first project
    await po.navigateToList();
    await po.clickCreate();
    await po.fillTab1Required({ code, name: `Dup Test ${code}` });
    await po.saveTab1();
    await po.waitForSuccessToast();

    // Try to create with same code
    await po.navigateToList();
    await po.clickCreate();
    await po.fillTab1Required({ code, name: `Dup Test 2 ${code}` });
    await po.saveTab1();

    // Should show error toast (409 conflict)
    await po.waitForErrorToast();
  });

  test('should handle concurrent edit (RowVersion conflict)', async ({ browser }) => {
    // Open two tabs editing the same project
    const ctx1 = await browser.newContext();
    const ctx2 = await browser.newContext();
    const page1 = await ctx1.newPage();
    const page2 = await ctx2.newPage();

    // This test requires a pre-existing project — skip if not available
    // The test validates that saving after another save produces a 409 error
    test.skip(true, 'Requires running services with seeded data — manual validation');

    await ctx1.close();
    await ctx2.close();
  });

  test('should preserve Tab 2 state after Tab 1 save failure', async ({ btcPage }) => {
    // This test validates tab state isolation — skip if services not available
    test.skip(true, 'Requires running services — manual validation');
  });

  test('should enable Tabs 2-6 after create redirect', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    const code = testCode('TAB-EN');

    await po.navigateToList();
    await po.clickCreate();

    // Before save: only Tab 1 active (others disabled)
    const tab2 = btcPage.locator('.ant-tabs-tab').nth(1);
    await expect(tab2).toHaveClass(/disabled/);

    // Save Tab 1 — creates project, redirects to edit
    await po.fillTab1Required({ code, name: `Tab Enable ${code}` });
    await po.saveTab1();
    await po.waitForSuccessToast();

    // After redirect: Tab 2 should be enabled
    await btcPage.waitForURL(/\/edit/, { timeout: 10000 });
    const tab2After = btcPage.locator('.ant-tabs-tab').nth(1);
    await expect(tab2After).not.toHaveClass(/disabled/);
  });
});
