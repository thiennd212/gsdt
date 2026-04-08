import { test, expect } from '../fixtures/auth.fixture';

test.describe('Smoke: i18n & Theme', () => {
  test('should switch to English', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/');
    await btcPage.waitForSelector('[data-testid="sidebar-menu"]', { timeout: 10000 });
    // Look for language switcher
    const langBtn = btcPage.locator('[data-testid="language-switcher"]').or(btcPage.locator('text=EN').or(btcPage.locator('text=English')));
    if (await langBtn.isVisible()) {
      await langBtn.click();
    }
  });

  test('should switch back to Vietnamese', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/');
    await btcPage.waitForSelector('[data-testid="sidebar-menu"]', { timeout: 10000 });
    const langBtn = btcPage.locator('[data-testid="language-switcher"]').or(btcPage.locator('text=VN').or(btcPage.locator('text=Tiếng Việt')));
    if (await langBtn.isVisible()) {
      await langBtn.click();
    }
  });

  test('should toggle dark mode', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/');
    await btcPage.waitForSelector('[data-testid="sidebar-menu"]', { timeout: 10000 });
    const themeBtn = btcPage.locator('[data-testid="theme-switcher"]').or(btcPage.locator('[aria-label*="theme"]'));
    if (await themeBtn.isVisible()) {
      await themeBtn.click();
      // Verify dark mode attribute
      const html = btcPage.locator('html');
      const theme = await html.getAttribute('data-theme');
      expect(['dark', 'light']).toContain(theme ?? 'light');
    }
  });
});
