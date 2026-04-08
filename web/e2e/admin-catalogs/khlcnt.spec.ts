import { test, expect } from '../fixtures/auth.fixture';
import { CatalogPage } from '../fixtures/page-objects/catalog.po';
import { testCode } from '../fixtures/test-data.fixture';

test.describe('Admin Catalogs: KHLCNT', () => {
  test('should create KHLCNT with 4 fields', async ({ btcPage }) => {
    const po = new CatalogPage(btcPage);
    await po.navigateToKhlcnt();
    await btcPage.getByRole('button', { name: 'Thêm mới' }).click();
    await btcPage.waitForSelector('.ant-modal', { state: 'visible' });

    // Fill 4 fields
    await btcPage.locator('.ant-modal input[id*="nameVi"]').fill(`KHLCNT Test ${testCode('KH')}`);
    await btcPage.locator('.ant-modal input[id*="nameEn"]').fill('E2E KHLCNT Test');
    await btcPage.locator('.ant-modal input[id*="signedBy"]').fill('Test Signer');
    // Date picker — type directly
    const dateInput = btcPage.locator('.ant-modal .ant-picker input');
    await dateInput.click();
    await dateInput.fill('01/01/2026');
    await dateInput.press('Enter');

    await btcPage.locator('.ant-modal .ant-btn-primary').click();
    await po.waitForSuccess();
  });

  test('should edit KHLCNT', async ({ btcPage }) => {
    test.skip(true, 'Requires seeded KHLCNT data');
  });

  test('should show KHLCNT in bid package combobox', async ({ btcPage }) => {
    test.skip(true, 'Requires created project + KHLCNT data');
  });
});
