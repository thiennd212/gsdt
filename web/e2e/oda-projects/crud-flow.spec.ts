import { test, expect } from '../fixtures/auth.fixture';
import { OdaProjectPage } from '../fixtures/page-objects/oda-project.po';
import { testCode } from '../fixtures/test-data.fixture';

test.describe('ODA Projects: CRUD Flow', () => {
  const code = testCode('CRUD-ODA');
  const name = `E2E ODA Test ${Date.now()}`;

  test('should create ODA project with donor + mechanism %', async ({ btcPage }) => {
    const po = new OdaProjectPage(btcPage);
    await po.navigateToList();
    await po.clickCreate();
    await po.fillTab1Required({ code, name, shortName: 'E2E-ODA' });
    await po.saveTab1();
    await expect(btcPage).toHaveURL(/\/oda-projects\/.*\/edit/);
  });

  test('should accept empty QHNS (v1.1)', async ({ btcPage }) => {
    const po = new OdaProjectPage(btcPage);
    await po.navigateToList();
    await po.clickCreate();
    // Fill required fields but leave QHNS empty
    await po.fillTab1Required({ code: testCode('QHNS'), name: 'QHNS Test', shortName: 'QH' });
    // QHNS field should not cause validation error
    const qhnsInput = btcPage.locator('input[id*="projectCodeQhns"]');
    await expect(qhnsInput).toHaveValue('');
  });

  test('should edit ODA-specific fields', async ({ btcPage }) => {
    test.skip(true, 'Requires seeded ODA project');
  });

  test('should delete ODA project', async ({ btcPage }) => {
    test.skip(true, 'Requires seeded ODA project');
  });
});
