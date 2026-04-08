import type { Page } from '@playwright/test';
import { antConfirmPopup, antWaitToast } from '../ant-helpers';

// Page object for admin catalog E2E tests
export class CatalogPage {
  constructor(private page: Page) {}

  async navigateToIndex() {
    await this.page.goto('http://localhost:6173/admin/catalogs');
    await this.page.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  }

  async navigateToGeneric(type: string) {
    await this.page.goto(`http://localhost:6173/admin/catalogs/${type}`);
    await this.page.waitForSelector('[data-testid="catalog-table"]', { timeout: 10000 });
  }

  async navigateToKhlcnt() {
    await this.page.goto('http://localhost:6173/admin/catalogs/contractor-selection-plans');
    await this.page.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  }

  async clickCreate() {
    await this.page.getByTestId('catalog-btn-create').click();
    await this.page.waitForSelector('.ant-modal', { state: 'visible', timeout: 5000 });
  }

  async fillGenericForm(code: string, name: string) {
    await this.page.locator('.ant-modal input[id*="code"]').fill(code);
    await this.page.locator('.ant-modal input[id*="name"]').fill(name);
  }

  async submitModal() {
    await this.page.locator('.ant-modal .ant-btn-primary').click();
  }

  async clickEdit(rowText: string) {
    const row = this.page.locator('tr.ant-table-row').filter({ hasText: rowText });
    await row.locator('button').first().click();
    await this.page.waitForSelector('.ant-modal', { state: 'visible', timeout: 5000 });
  }

  async clickDelete(rowText: string) {
    const row = this.page.locator('tr.ant-table-row').filter({ hasText: rowText });
    await row.locator('button[class*="danger"]').click();
    await antConfirmPopup(this.page);
  }

  async waitForSuccess(text?: string) { await antWaitToast(this.page, text ?? 'thành công'); }
  async waitForError(text?: string) { await antWaitToast(this.page, text ?? 'thất bại'); }
}
