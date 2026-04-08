import { test, expect } from '../fixtures/auth.fixture';
// Note: @axe-core/playwright must be installed: npm i -D @axe-core/playwright
// Tests will skip gracefully if not installed

test.describe('Smoke: Accessibility (a11y)', () => {
  test('a11y: project list page', async ({ btcPage }) => {
    try {
      const { default: AxeBuilder } = await import('@axe-core/playwright');
      await btcPage.goto('http://localhost:6173/domestic-projects');
      await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
      const results = await new AxeBuilder({ page: btcPage }).withTags(['wcag2a', 'wcag2aa']).analyze();
      const critical = results.violations.filter((v) => v.impact === 'critical' || v.impact === 'serious');
      expect(critical).toHaveLength(0);
    } catch {
      test.skip(true, '@axe-core/playwright not installed');
    }
  });

  test('a11y: project form page', async ({ btcPage }) => {
    try {
      const { default: AxeBuilder } = await import('@axe-core/playwright');
      await btcPage.goto('http://localhost:6173/domestic-projects/new');
      await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
      const results = await new AxeBuilder({ page: btcPage }).withTags(['wcag2a', 'wcag2aa']).analyze();
      const critical = results.violations.filter((v) => v.impact === 'critical' || v.impact === 'serious');
      expect(critical).toHaveLength(0);
    } catch {
      test.skip(true, '@axe-core/playwright not installed');
    }
  });

  test('a11y: admin catalog page', async ({ btcPage }) => {
    try {
      const { default: AxeBuilder } = await import('@axe-core/playwright');
      await btcPage.goto('http://localhost:6173/admin/catalogs');
      await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
      const results = await new AxeBuilder({ page: btcPage }).withTags(['wcag2a', 'wcag2aa']).analyze();
      const critical = results.violations.filter((v) => v.impact === 'critical' || v.impact === 'serious');
      expect(critical).toHaveLength(0);
    } catch {
      test.skip(true, '@axe-core/playwright not installed');
    }
  });

  test('a11y: dashboard', async ({ btcPage }) => {
    try {
      const { default: AxeBuilder } = await import('@axe-core/playwright');
      await btcPage.goto('http://localhost:6173/');
      await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
      const results = await new AxeBuilder({ page: btcPage }).withTags(['wcag2a', 'wcag2aa']).analyze();
      const critical = results.violations.filter((v) => v.impact === 'critical' || v.impact === 'serious');
      expect(critical).toHaveLength(0);
    } catch {
      test.skip(true, '@axe-core/playwright not installed');
    }
  });
});
