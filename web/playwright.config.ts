import { defineConfig, devices } from '@playwright/test';

// Playwright E2E configuration — targets local dev servers (BE:6001, Auth:6002, FE:6173).
// Run with: npm run test:e2e (requires all 3 services running).
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,

  reporter: [['html', { open: 'never' }]],

  use: {
    baseURL: 'http://localhost:6173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    // Existing compliance/perf projects (preserved from corebase)
    {
      name: 'compliance-qd742',
      testMatch: /qd742-compliance/,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'compliance-pdpl',
      testMatch: /pdpl-compliance/,
      dependencies: ['compliance-qd742'],
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'browser-ui',
      testMatch: /browser-ui\.spec/,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'signalr',
      testMatch: /signalr/,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'admin-browser',
      testMatch: /browser-ui\//,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'e2e-perf',
      testMatch: /performance-baselines/,
      use: { ...devices['Desktop Chrome'] },
    },
    // GSDT E2E tests
    {
      name: 'gsdt-e2e',
      testDir: './e2e',
      testMatch: /\.(spec|test)\.(ts|tsx)$/,
      testIgnore: /compliance|browser-ui|signalr|performance-baselines/,
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
