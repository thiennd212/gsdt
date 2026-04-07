# Phase 2: Public Form Flows

## Overview
- **Priority:** P1 — backlog priority, citizen-facing feature
- **Effort:** 3h
- **Status:** Done
- **Dependencies:** Phase 1 (POM + Fixtures)
- **Blocks:** None

## Context
- Route: `/public/forms/$code` — anonymous access, outside authenticated layout
- Admin creates form template → publishes → citizen fills via public URL → submits → admin reviews
- Existing API tests: `form-submissions.spec.ts` (API-only CRUD)
- Missing: browser-level multi-step flow, conditional fields, file upload, CSV export

## Key Insights
- Public form page is outside `authenticatedRoute` — no auth required for citizen
- Admin side needs auth to create template, review submissions
- Form templates have `status: Active` flag — only active templates have public URLs
- Form code (not ID) used in public URL path
- Vietnamese labels expected in form UI

## Files to Create

| File | Purpose | LOC est |
|------|---------|---------|
| `e2e/public-form-lifecycle.spec.ts` | Full lifecycle: admin create → publish → citizen fill → submit → admin review | ~150 |
| `e2e/public-form-validation.spec.ts` | Validation errors, required fields, conditional logic | ~100 |
| `e2e/public-form-file-upload.spec.ts` | File upload in public form, size/type validation | ~80 |
| `e2e/public-form-export.spec.ts` | CSV export of submissions from admin side | ~60 |

## Data Flow

```
Admin (authedPage)                    Citizen (anonPage)
  |                                      |
  |-- POST /forms/templates              |
  |-- PATCH status=Active                |
  |-- get template.code ─────────────────|
  |                                      |-- GET /public/forms/{code}
  |                                      |-- fill fields
  |                                      |-- POST submit
  |                                      |-- see confirmation
  |                                      |
  |-- GET /forms/submissions ←───────────|
  |-- verify submission data             |
  |-- CSV export                         |
```

## Implementation Steps

### 1. `e2e/public-form-lifecycle.spec.ts` — Full E2E lifecycle

```typescript
test.describe('Public form lifecycle', () => {
  test.describe.configure({ mode: 'serial' });
  
  let templateId: string;
  let formCode: string;
  
  test('admin creates form template via API', async ({ authedPage, apiToken, request }) => {
    // POST /api/v1/forms/templates with fields config
    // Store templateId + formCode
  });
  
  test('admin publishes template (set Active)', async ({ apiToken, request }) => {
    // PATCH /api/v1/forms/templates/{id} status=Active
  });
  
  test('citizen can access public form without auth', async ({ browser }) => {
    const ctx = await browser.newContext(); // no auth
    const page = await ctx.newPage();
    const publicForm = new PublicFormPage(page);
    await publicForm.goto(formCode);
    // Assert form renders with expected fields
  });
  
  test('citizen fills and submits form', async ({ browser }) => {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    const publicForm = new PublicFormPage(page);
    await publicForm.goto(formCode);
    await publicForm.fillTextField('Họ và tên', 'Nguyen Van Test');
    await publicForm.fillTextField('Email', 'test@citizen.vn');
    await publicForm.submit();
    const msg = await publicForm.getConfirmationMessage();
    expect(msg).toContain('thành công');
  });
  
  test('admin sees submission in list', async ({ authedPage, apiToken, request }) => {
    // GET /api/v1/forms/submissions?templateId={id}
    // Assert contains the citizen's data
  });
  
  test('cleanup: delete template', async ({ apiToken, request }) => {
    // DELETE /api/v1/forms/templates/{id}
  });
});
```

### 2. `e2e/public-form-validation.spec.ts` — Validation & conditional fields

```typescript
test.describe('Public form validation', () => {
  test('required field shows error when empty', async ({ browser }) => {
    // Submit without filling required field
    // Assert validation error visible
  });
  
  test('email field validates format', async ({ browser }) => {
    // Fill with invalid email
    // Assert format error
  });
  
  test('conditional field shows/hides based on trigger', async ({ browser }) => {
    // Select option that triggers conditional field
    // Assert conditional field becomes visible
    // Change selection
    // Assert conditional field hidden
  });
  
  test('multi-step form validates per step', async ({ browser }) => {
    // Try to advance without filling required step 1 fields
    // Assert blocked with validation message
  });
});
```

### 3. `e2e/public-form-file-upload.spec.ts` — File upload scenarios

```typescript
test.describe('Public form file upload', () => {
  test('upload valid file succeeds', async ({ browser }) => {
    // Create test file, upload via form
    // Assert upload indicator visible
  });
  
  test('upload oversized file shows error', async ({ browser }) => {
    // Create >10MB file, attempt upload
    // Assert size limit error
  });
  
  test('upload invalid file type shows error', async ({ browser }) => {
    // Upload .exe file
    // Assert type restriction error
  });
  
  test('multiple file upload works', async ({ browser }) => {
    // Upload 2+ files
    // Assert all listed
  });
});
```

### 4. `e2e/public-form-export.spec.ts` — CSV export

```typescript
test.describe('Form submissions CSV export', () => {
  test('admin exports submissions as CSV', async ({ authedPage }) => {
    // Navigate to form submissions page
    // Click export button
    // Intercept download, verify CSV content
    const download = await authedPage.waitForEvent('download');
    expect(download.suggestedFilename()).toContain('.csv');
  });
  
  test('exported CSV contains expected columns', async ({ authedPage }) => {
    // Download + parse CSV
    // Assert headers match template fields
  });
});
```

## Test Matrix

| Test | Scenario | Expected Result |
|------|----------|----------------|
| lifecycle/create-template | Admin creates template via API | 200, templateId returned |
| lifecycle/publish | Admin sets Active status | 200, template accessible |
| lifecycle/citizen-access | Anonymous visits /public/forms/{code} | Form renders, no auth required |
| lifecycle/citizen-submit | Fill all fields, submit | Confirmation message visible |
| lifecycle/admin-review | Admin checks submissions list | Citizen data appears |
| validation/required-empty | Submit with empty required field | Error shown, form not submitted |
| validation/email-format | Enter "not-an-email" | Format validation error |
| validation/conditional | Select trigger value | Dependent field appears |
| validation/multi-step | Advance without filling step 1 | Blocked, error on step 1 |
| upload/valid | Upload PDF <5MB | Success indicator |
| upload/oversized | Upload >10MB file | Size limit error |
| upload/invalid-type | Upload .exe | Type restriction error |
| export/csv-download | Click export | .csv file downloaded |
| export/csv-content | Parse downloaded CSV | Headers match template fields |

## Success Criteria
- [x] Full lifecycle test passes: create → publish → fill → submit → review
- [x] Validation errors visible for required fields, email format
- [x] Conditional field show/hide works
- [x] File upload succeeds for valid files, rejects invalid
- [x] CSV export produces valid file with correct columns
- [x] All tests run without auth for citizen side (anonymous context)
- [x] Cleanup removes all seeded data

## Risk Assessment
| Risk | Mitigation |
|------|------------|
| No Active templates in dev env | Test creates its own template + cleanup |
| Public form route may not render without backend form config | Pre-check API for template existence, skip if 404 |
| File upload dialog differs by OS | Use Playwright `setInputFiles()` not native dialog |
| CSV export may be async (queued job) | Poll submissions export endpoint, 30s timeout |

## Security Considerations
- Public form tests must NOT use authenticated context (verify anonymous access)
- Admin operations must use authenticated context
- No hardcoded sensitive data beyond dev credentials already in auth-helper.ts
