import { test, expect } from '../fixtures/auth.fixture';
import { testCode } from '../fixtures/test-data.fixture';
import { DomesticProjectPage } from '../fixtures/page-objects/domestic-project.po';

test.describe('Domestic Projects: Tab Navigation & Sub-entities', () => {
  test('should show saved/unsaved badges on tabs', async ({ btcPage }) => {
    // Create project first, then verify tab badges
    const po = new DomesticProjectPage(btcPage);
    const code = testCode('BADGE');

    await po.navigateToList();
    await po.clickCreate();
    await po.fillTab1Required({ code, name: `Badge Test ${code}` });
    await po.saveTab1();
    await po.waitForSuccessToast();

    // Tab 1 should show green checkmark after save
    const tab1 = btcPage.locator('.ant-tabs-tab').first();
    await expect(tab1.locator('svg')).toBeVisible(); // CheckCircleFilled icon
  });

  test('should add/delete location in Tab 1 Zone 4', async ({ btcPage }) => {
    test.skip(true, 'Requires running services with seeded province data');
  });

  test('should add investment decision in Tab 1 Zone 5', async ({ btcPage }) => {
    test.skip(true, 'Requires running services with project context');
  });

  test('should add bid package via popup in Tab 2', async ({ btcPage }) => {
    test.skip(true, 'Requires running services with project + KHLCNT data');
  });
});
