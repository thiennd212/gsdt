// form-admin-editors.test.tsx — smoke tests for admin editor components
// Tests: FormSettingsPanel, RequiredIfEditor, DataSourceEditor

import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Form } from 'antd';

// ──────────────────────────────────────────────────────────────────────────────
// Global mocks
// ──────────────────────────────────────────────────────────────────────────────

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key, i18n: { language: 'en' } }),
}));

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
  useParams: () => ({ id: 'test-template-id' }),
  useLocation: () => ({ pathname: '/' }),
  Link: ({ children }: { children: ReactNode }) => children,
}));

vi.mock('@/features/auth/use-auth', () => ({
  useAuth: () => ({
    user: { profile: { tenant_id: 'test-tenant', sub: 'test-user', name: 'Test User' } },
    isAuthenticated: true,
  }),
}));

// Mock form-api hooks for admin editors
vi.mock('@/features/forms/form-api', () => ({
  useUpdateTemplate: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
  useWorkflowDefinitions: () => ({
    data: [
      { id: 'wf1', name: 'Standard Review' },
      { id: 'wf2', name: 'Manager Approval' },
    ],
    isLoading: false,
  }),
  useFieldOptions: () => ({
    data: [],
    isLoading: false,
  }),
}));

// ──────────────────────────────────────────────────────────────────────────────
// Component imports — after mocks
// ──────────────────────────────────────────────────────────────────────────────

import { FormSettingsPanel } from '@/features/forms/form-settings-panel';
import { RequiredIfEditor } from '@/features/forms/builder/required-if-editor';
import { DataSourceEditor } from '@/features/forms/builder/data-source-editor';
import type { FormTemplateDto, FormFieldDto } from '@/features/forms/form-types';

// ──────────────────────────────────────────────────────────────────────────────
// Helper: create mock template
// ──────────────────────────────────────────────────────────────────────────────

function makeTemplate(overrides: Partial<FormTemplateDto> = {}): FormTemplateDto {
  return {
    id: 'tpl-1',
    code: 'FORM-001',
    name: 'Test Form',
    nameVi: 'Biểu mẫu thử',
    status: 'Draft',
    version: 1,
    fields: [],
    submissionsCount: 0,
    createdAt: '2024-01-01T00:00:00Z',
    requiresConsent: false,
    ...overrides,
  };
}

// Helper: create mock field
function makeField(overrides: Partial<FormFieldDto> = {}): FormFieldDto {
  return {
    id: 'f1',
    fieldKey: 'test_field',
    type: 'Text',
    labelVi: 'Test Label',
    labelEn: 'Test Label',
    required: false,
    displayOrder: 1,
    isActive: true,
    addedInVersion: 1,
    ...overrides,
  };
}

// ──────────────────────────────────────────────────────────────────────────────
// Wrapper utility
// ──────────────────────────────────────────────────────────────────────────────

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-EDITOR-001: FormSettingsPanel
// ──────────────────────────────────────────────────────────────────────────────

describe('FormSettingsPanel — TC-FE-EDITOR-001', () => {
  it('renders without crashing', () => {
    const template = makeTemplate();
    const { container } = render(
      <FormSettingsPanel template={template} />,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });

  it('renders consent toggle switch', () => {
    const template = makeTemplate({ requiresConsent: false });
    const { container } = render(
      <FormSettingsPanel template={template} />,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('.ant-switch')).toBeTruthy();
  });

  it('renders workflow dropdown', () => {
    const template = makeTemplate();
    const { container } = render(
      <FormSettingsPanel template={template} />,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('.ant-select')).toBeTruthy();
  });

  it('renders settings form with inputs', () => {
    const template = makeTemplate();
    const { container } = render(
      <FormSettingsPanel template={template} />,
      { wrapper: makeWrapper() }
    );
    // Should have form structure with inputs
    expect(container.querySelector('.ant-form')).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-EDITOR-002: RequiredIfEditor
// ──────────────────────────────────────────────────────────────────────────────

// Mock component to wrap RequiredIfEditor
function RequiredIfEditorWrapper({ otherFields, disabled }: { otherFields: any[]; disabled: boolean }) {
  const [form] = Form.useForm();
  return <RequiredIfEditor form={form} otherFields={otherFields} disabled={disabled} />;
}

describe('RequiredIfEditor — TC-FE-EDITOR-002', () => {
  it('renders editor component', () => {
    const otherFields = [
      makeField({ id: 'f1', fieldKey: 'status' }),
      makeField({ id: 'f2', fieldKey: 'type' }),
    ];
    const { container } = render(
      <Form>
        <RequiredIfEditorWrapper otherFields={otherFields} disabled={false} />
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });

  it('renders with multiple fields available', () => {
    const otherFields = [
      makeField({ id: 'f1', fieldKey: 'status', labelVi: 'Trạng thái' }),
      makeField({ id: 'f2', fieldKey: 'type', labelVi: 'Loại' }),
    ];
    const { container } = render(
      <Form>
        <RequiredIfEditorWrapper otherFields={otherFields} disabled={false} />
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container).toBeTruthy();
  });

  it('renders with empty fields list', () => {
    const { container } = render(
      <Form>
        <RequiredIfEditorWrapper otherFields={[]} disabled={false} />
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });

  it('renders with disabled=true', () => {
    const otherFields = [makeField({ id: 'f1', fieldKey: 'field1' })];
    const { container } = render(
      <Form>
        <RequiredIfEditorWrapper otherFields={otherFields} disabled={true} />
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });

  it('renders with disabled=false', () => {
    const otherFields = [makeField({ id: 'f1', fieldKey: 'field1' })];
    const { container } = render(
      <Form>
        <RequiredIfEditorWrapper otherFields={otherFields} disabled={false} />
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-EDITOR-003: DataSourceEditor — InternalRef mode
// ──────────────────────────────────────────────────────────────────────────────

describe('DataSourceEditor — TC-FE-EDITOR-003', () => {
  it('renders module Select for InternalRef fieldType', () => {
    const { container } = render(
      <Form>
        <Form.Item name="dataSource">
          <DataSourceEditor fieldType="InternalRef" />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('.ant-select')).toBeTruthy();
  });

  it('renders entity Input for InternalRef fieldType', () => {
    const { container } = render(
      <Form>
        <Form.Item name="dataSource">
          <DataSourceEditor fieldType="InternalRef" />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('input')).toBeTruthy();
  });

  it('renders without crashing', () => {
    const { container } = render(
      <Form>
        <Form.Item name="dataSource">
          <DataSourceEditor fieldType="InternalRef" disabled={false} />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-EDITOR-004: DataSourceEditor — ExternalRef mode
// ──────────────────────────────────────────────────────────────────────────────

describe('DataSourceEditor — TC-FE-EDITOR-004', () => {
  it('renders URL Input for ExternalRef fieldType', () => {
    const { container } = render(
      <Form>
        <Form.Item name="dataSource">
          <DataSourceEditor fieldType="ExternalRef" />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('input')).toBeTruthy();
  });

  it('renders HTTP method Select for ExternalRef fieldType', () => {
    const { container } = render(
      <Form>
        <Form.Item name="dataSource">
          <DataSourceEditor fieldType="ExternalRef" />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    // Should have at least one select for method
    expect(container.querySelector('.ant-select')).toBeTruthy();
  });

  it('renders without crashing', () => {
    const { container } = render(
      <Form>
        <Form.Item name="dataSource">
          <DataSourceEditor fieldType="ExternalRef" disabled={true} />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });
});
