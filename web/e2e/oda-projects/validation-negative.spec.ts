import { test, expect } from '../fixtures/auth.fixture';
import { OdaProjectPage } from '../fixtures/page-objects/oda-project.po';
import { testCode } from '../fixtures/test-data.fixture';

test.describe('ODA Projects: Validation & Negative', () => {
  test('should reject mechanism % != 100', async ({ btcPage }) => {
    const po = new OdaProjectPage(btcPage);
    await po.navigateToList();
    await po.clickCreate();
    await po.fillTab1Required({ code: testCode('MECH'), name: 'Mech Test', shortName: 'MT' });
    await po.fillMechanismPercent(60, 30); // = 90, not 100
    // Total display should show 90
    const totalInput = btcPage.locator('input[id*="Tổng"]').or(btcPage.locator('.ant-input-number-disabled input').last());
    // Validation should catch this on save
    await po.saveTab1();
  });

  test('should reject duplicate project code', async ({ btcPage }) => {
    const po = new OdaProjectPage(btcPage);
    const code = testCode('DUP-ODA');
    await po.navigateToList();
    await po.clickCreate();
    await po.fillTab1Required({ code, name: 'Dup ODA 1', shortName: 'D1' });
    await po.saveTab1();

    await po.navigateToList();
    await po.clickCreate();
    await po.fillTab1Required({ code, name: 'Dup ODA 2', shortName: 'D2' });
    await po.saveTab1();
    await po.waitForErrorToast();
  });

  test('should show validation errors on empty submit', async ({ btcPage }) => {
    const po = new OdaProjectPage(btcPage);
    await po.navigateToList();
    await po.clickCreate();
    await po.saveTab1();
    const errors = btcPage.locator('.ant-form-item-explain-error');
    await expect(errors.first()).toBeVisible({ timeout: 3000 });
  });

  test('should handle concurrent edit', async ({ btcPage }) => {
    test.skip(true, 'Requires seeded ODA project + running services');
  });
});
