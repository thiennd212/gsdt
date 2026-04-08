import type { Page, Locator } from '@playwright/test';

// Ant Design interaction helpers for Playwright E2E tests.
// Ant components use complex DOM (portals, virtual scroll, animations).
// These helpers abstract the interaction pattern for reliability.

/** Open an Ant Select dropdown, search for option, and click it */
export async function antSelect(page: Page, testId: string, optionText: string): Promise<void> {
  const select = page.getByTestId(testId);
  await select.click();
  // Wait for dropdown portal to appear
  await page.waitForSelector('.ant-select-dropdown', { state: 'visible', timeout: 5000 });
  // Click the matching option
  await page.locator('.ant-select-item-option').filter({ hasText: optionText }).click();
}

/** Clear an Ant Select dropdown */
export async function antSelectClear(page: Page, testId: string): Promise<void> {
  const select = page.getByTestId(testId);
  await select.hover();
  const clear = select.locator('.ant-select-clear');
  if (await clear.isVisible()) {
    await clear.click();
  }
}

/** Pick a date in an Ant DatePicker by typing formatted date */
export async function antDatePick(page: Page, testId: string, date: string): Promise<void> {
  const picker = page.getByTestId(testId);
  await picker.click();
  const input = picker.locator('input');
  await input.fill(date);
  await input.press('Enter');
}

/** Confirm an Ant Popconfirm dialog */
export async function antConfirmPopup(page: Page): Promise<void> {
  await page.waitForSelector('.ant-popconfirm', { state: 'visible', timeout: 5000 });
  await page.locator('.ant-popconfirm-buttons .ant-btn-primary').click();
  await page.waitForSelector('.ant-popconfirm', { state: 'hidden', timeout: 5000 });
}

/** Wait for an Ant Modal to fully close */
export async function antWaitModalClose(page: Page): Promise<void> {
  await page.waitForSelector('.ant-modal-mask', { state: 'hidden', timeout: 5000 });
}

/** Wait for an Ant message toast to appear and verify text */
export async function antWaitToast(page: Page, text: string): Promise<void> {
  await page.waitForSelector(`.ant-message-notice:has-text("${text}")`, { timeout: 5000 });
}

/** Wait for table to finish loading (spinner disappears) */
export async function antWaitTableLoad(page: Page, testId: string): Promise<void> {
  const table = page.getByTestId(testId);
  // Wait for Ant spinning overlay to disappear
  await table.locator('.ant-spin-spinning').waitFor({ state: 'hidden', timeout: 10000 }).catch(() => {});
}

/** Get row count from an Ant Table */
export async function antTableRowCount(page: Page, testId: string): Promise<number> {
  const table = page.getByTestId(testId);
  return table.locator('tbody tr.ant-table-row').count();
}
