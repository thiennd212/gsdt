import type { Page } from '@playwright/test';
import { antConfirmPopup, antWaitToast, antTableRowCount } from '../ant-helpers';

const BASE = '/oda-projects';

// Page object for ODA project E2E tests
export class OdaProjectPage {
  constructor(private page: Page) {}

  async navigateToList() {
    await this.page.goto(`http://localhost:6173${BASE}`);
    await this.page.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  }

  async clickCreate() {
    await this.page.getByRole('button', { name: 'Thêm mới' }).click();
    await this.page.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  }

  async fillTab1Required(data: { code: string; name: string; shortName: string }) {
    await this.page.locator('input[id*="projectCode"]').fill(data.code);
    await this.page.locator('input[id*="projectName"]').fill(data.name);
    await this.page.locator('input[id*="shortName"]').fill(data.shortName);
  }

  async fillMechanismPercent(grant: number, relending: number) {
    await this.page.locator('input[id*="grantMechanismPercent"]').fill(String(grant));
    await this.page.locator('input[id*="relendingMechanismPercent"]').fill(String(relending));
  }

  async saveTab1() {
    await this.page.getByRole('button', { name: 'Lưu thông tin' }).first().click();
  }

  async getTableRowCount(): Promise<number> {
    return this.page.locator('tr.ant-table-row').count();
  }

  async clickAction(rowText: string, action: 'view' | 'edit' | 'delete') {
    const row = this.page.locator('tr.ant-table-row').filter({ hasText: rowText });
    if (action === 'delete') {
      await row.locator('button[class*="danger"]').click();
      await antConfirmPopup(this.page);
    } else {
      const idx = action === 'view' ? 0 : 1;
      await row.locator('button').nth(idx).click();
    }
  }

  async waitForSuccessToast(text?: string) { await antWaitToast(this.page, text ?? 'thành công'); }
  async waitForErrorToast(text?: string) { await antWaitToast(this.page, text ?? 'thất bại'); }
}
