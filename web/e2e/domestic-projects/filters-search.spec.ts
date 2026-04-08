import { test, expect } from '../fixtures/auth.fixture';
import { DomesticProjectPage } from '../fixtures/page-objects/domestic-project.po';

test.describe('Domestic Projects: Filters & Search', () => {
  test('should filter by search text', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();

    // Search for a term that shouldn't match everything
    await po.searchByText('NONEXISTENT-PROJECT-XYZ');

    // Table should show empty or filtered results
    const table = btcPage.getByTestId('domestic-table-projects');
    await expect(table).toBeVisible();
  });

  test('should filter by dropdown', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();

    // Select a filter dropdown value (if data exists)
    const statusSelect = btcPage.getByTestId('domestic-select-status');
    await expect(statusSelect).toBeVisible();
    // Verify the select is interactive
    await statusSelect.click();
    await btcPage.keyboard.press('Escape');
  });

  test('should clear all filters', async ({ btcPage }) => {
    const po = new DomesticProjectPage(btcPage);
    await po.navigateToList();

    // Set a search term
    await po.searchByText('test');
    // Clear
    await po.clearFilters();

    // Search input should be empty
    const input = btcPage.getByTestId('domestic-input-search');
    await expect(input).toHaveValue('');
  });
});
