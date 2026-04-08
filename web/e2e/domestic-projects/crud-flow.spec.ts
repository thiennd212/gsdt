import { test, expect } from '../fixtures/auth.fixture';
import { DomesticProjectPage } from '../fixtures/page-objects/domestic-project.po';
import { testCode } from '../fixtures/test-data.fixture';

test.describe('Domestic Projects: CRUD Flow', () => {
  const projectCode = testCode('CRUD-TN');
  const projectName = `E2E CRUD Test ${Date.now()}`;

  test('should create domestic project via Tab 1', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();
    await po.clickCreate();

    await po.fillTab1Required({ code: projectCode, name: projectName });
    await po.saveTab1();

    // Should redirect to edit page with all tabs enabled
    await expect(btcPage).toHaveURL(/\/domestic-projects\/.*\/edit/);
    await po.waitForSuccessToast('thành công');
  });

  test('should list created project in datagrid', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();
    await po.searchByText(projectCode);

    const table = btcPage.getByTestId('domestic-table-projects');
    await expect(table).toContainText(projectCode);
  });

  test('should edit project Tab 1 fields', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();
    await po.searchByText(projectCode);
    await po.clickAction(projectCode, 'edit');

    // Change project name
    const nameInput = btcPage.locator('input[id*="projectName"]');
    await nameInput.clear();
    await nameInput.fill(`${projectName} Updated`);
    await po.saveTab1();
    await po.waitForSuccessToast('Cập nhật');
  });

  test('should view project detail (readonly)', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();
    await po.searchByText(projectCode);
    await po.clickAction(projectCode, 'view');

    // All form inputs should be disabled
    const disabled = await po.isFormDisabled();
    expect(disabled).toBeTruthy();

    // Should show edit button
    await expect(btcPage.getByRole('button', { name: 'Chỉnh sửa' })).toBeVisible();
  });

  test('should soft-delete project', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();
    await po.searchByText(projectCode);
    await po.clickAction(projectCode, 'delete');
    await po.waitForSuccessToast('Đã xóa');
  });
});
