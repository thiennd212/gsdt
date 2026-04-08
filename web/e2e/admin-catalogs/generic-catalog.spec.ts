import { test, expect } from '../fixtures/auth.fixture';
import { CatalogPage } from '../fixtures/page-objects/catalog.po';
import { testCode } from '../fixtures/test-data.fixture';

test.describe('Admin Catalogs: Generic CRUD', () => {
  const code = testCode('CAT');
  const name = `E2E Test Catalog ${Date.now()}`;

  test('should navigate catalog index to generic catalog', async ({ btcPage }) => {
    const po = new CatalogPage(btcPage);
    await po.navigateToIndex();
    // Click first catalog card
    const card = btcPage.locator('.ant-card').first();
    await card.click();
    await expect(btcPage).toHaveURL(/\/admin\/catalogs\//);
  });

  test('should create generic catalog item', async ({ btcPage }) => {
    const po = new CatalogPage(btcPage);
    await po.navigateToGeneric('managing-authorities');
    await po.clickCreate();
    await po.fillGenericForm(code, name);
    await po.submitModal();
    await po.waitForSuccess('Thêm mới');
  });

  test('should edit catalog item', async ({ btcPage }) => {
    const po = new CatalogPage(btcPage);
    await po.navigateToGeneric('managing-authorities');
    // Find and edit the item we created
    await po.clickEdit(code);
    await btcPage.locator('.ant-modal input[id*="name"]').clear();
    await btcPage.locator('.ant-modal input[id*="name"]').fill(`${name} Updated`);
    await po.submitModal();
    await po.waitForSuccess('Cập nhật');
  });

  test('should soft-delete catalog item', async ({ btcPage }) => {
    const po = new CatalogPage(btcPage);
    await po.navigateToGeneric('managing-authorities');
    await po.clickDelete(code);
    await po.waitForSuccess('Xóa');
  });
});
