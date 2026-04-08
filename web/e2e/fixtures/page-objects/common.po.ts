import type { Page } from '@playwright/test';

// Common page object helpers shared across all E2E test suites

export class CommonPage {
  constructor(protected page: Page) {}

  /** Navigate to a route and wait for page header */
  async navigateTo(path: string): Promise<void> {
    await this.page.goto(`http://localhost:6173${path}`);
    await this.page.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
  }

  /** Check sidebar menu is visible (confirms auth) */
  async isSidebarVisible(): Promise<boolean> {
    return this.page.getByTestId('sidebar-menu').isVisible();
  }

  /** Check breadcrumb is visible */
  async isBreadcrumbVisible(): Promise<boolean> {
    return this.page.getByTestId('breadcrumb').isVisible();
  }

  /** Get page header title text */
  async getPageTitle(): Promise<string> {
    const header = this.page.getByTestId('page-header');
    return header.locator('h4').innerText();
  }

  /** Wait for success toast */
  async waitForSuccessToast(text?: string): Promise<void> {
    const selector = text
      ? `.ant-message-success:has-text("${text}")`
      : '.ant-message-success';
    await this.page.waitForSelector(selector, { timeout: 5000 });
  }

  /** Wait for error toast */
  async waitForErrorToast(text?: string): Promise<void> {
    const selector = text
      ? `.ant-message-error:has-text("${text}")`
      : '.ant-message-error';
    await this.page.waitForSelector(selector, { timeout: 5000 });
  }
}
