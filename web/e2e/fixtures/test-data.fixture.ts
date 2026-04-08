import type { APIRequestContext } from '@playwright/test';

const API_URL = 'http://localhost:6001';

// Unique prefix per test run to prevent data collision
export const RUN_ID = `e2e-${Date.now()}`;

// Generate a unique test project code
export function testCode(suffix: string): string {
  return `${RUN_ID}-${suffix}`;
}

// Cleanup all test data with the run prefix via BE cleanup endpoint
export async function cleanupTestData(request: APIRequestContext): Promise<void> {
  await request.delete(`${API_URL}/api/v1/test/cleanup?prefix=${RUN_ID}`);
}

// Create a domestic project via API for test setup
export async function seedDomesticProject(
  request: APIRequestContext,
  overrides: Record<string, unknown> = {},
): Promise<{ id: string }> {
  const res = await request.post(`${API_URL}/api/v1/domestic-projects`, {
    data: {
      projectCode: testCode('DA-TN'),
      projectName: `E2E Test Project ${RUN_ID}`,
      managingAuthorityId: '00000000-0000-0000-0000-000000000000',
      industrySectorId: '00000000-0000-0000-0000-000000000000',
      projectOwnerId: '00000000-0000-0000-0000-000000000000',
      projectGroupId: '00000000-0000-0000-0000-000000000000',
      subProjectType: 0,
      prelimCentralBudget: 100,
      prelimLocalBudget: 50,
      prelimOtherPublicCapital: 25,
      prelimOtherCapital: 10,
      statusId: '00000000-0000-0000-0000-000000000000',
      ...overrides,
    },
    headers: {
      'Content-Type': 'application/json',
      // TestAuthHandler headers for API calls outside browser context
      'X-Test-UserId': '10000000-0000-0000-0000-000000000001',
      'X-Test-TenantId': '00000000-0000-0000-0000-000000000001',
      'X-Test-Roles': 'Admin,BTC',
    },
  });
  return res.json().then((r) => r.data ?? r);
}
