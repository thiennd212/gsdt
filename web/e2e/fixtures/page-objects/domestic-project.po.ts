import type { Page } from '@playwright/test';
import { antSelect, antConfirmPopup, antWaitToast, antTableRowCount } from '../ant-helpers';

const BASE = '/domestic-projects';

// Page object for domestic project E2E tests
export class DomesticProjectPage {
  constructor(private page: Page) {}

  async navigateToList() {
    await this.page.goto(`http://localhost:6173${BASE}`);
    await this.page.waitForSelector('[data-testid="domestic-table-projects"]', { timeout: 10000 });
  }

  async clickCreate() {
    await this.page.getByTestId('domestic-btn-create').click();
    await this.page.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  }

  async fillTab1Required(data: {
    code: string;
    name: string;
  }) {
    await this.page.locator('input[id*="projectCode"]').fill(data.code);
    await this.page.locator('input[id*="projectName"]').fill(data.name);
  }

  async saveTab1() {
    await this.page.getByRole('button', { name: 'Lưu thông tin' }).first().click();
  }

  async clickBack() {
    await this.page.getByRole('button', { name: 'Quay lại' }).click();
  }

  async getTableRowCount(): Promise<number> {
    return antTableRowCount(this.page, 'domestic-table-projects');
  }

  async searchByText(query: string) {
    await this.page.getByTestId('domestic-input-search').fill(query);
    await this.page.getByTestId('domestic-btn-search').click();
  }

  async clearFilters() {
    await this.page.getByTestId('domestic-btn-clear').click();
  }

  async clickAction(rowText: string, action: 'view' | 'edit' | 'delete') {
    const row = this.page.locator('tr.ant-table-row').filter({ hasText: rowText });
    const icons = { view: 'EyeOutlined', edit: 'EditOutlined', delete: 'DeleteOutlined' };
    if (action === 'delete') {
      await row.locator('button[class*="danger"]').click();
      await antConfirmPopup(this.page);
    } else {
      const btnIndex = action === 'view' ? 0 : 1;
      await row.locator('button').nth(btnIndex).click();
    }
  }

  async waitForSuccessToast(text?: string) {
    await antWaitToast(this.page, text ?? 'thành công');
  }

  async waitForErrorToast(text?: string) {
    await antWaitToast(this.page, text ?? 'thất bại');
  }

  async isFormDisabled(): Promise<boolean> {
    const input = this.page.locator('input[id*="projectCode"]');
    return input.isDisabled();
  }
}
