import { test, expect } from '../fixtures/auth.fixture';

test.describe('Identity: Profile', () => {
  test('should view own profile', async ({ btcPage }) => {
    await btcPage.goto('http://localhost:6173/profile');
    await btcPage.waitForSelector('[data-testid="page-header"]', { timeout: 10000 });
    await expect(btcPage.getByTestId('page-header')).toBeVisible();
  });

  test('should update profile fields', async ({ btcPage }) => {
    test.skip(true, 'Requires running auth service with user profile API');
  });
});
