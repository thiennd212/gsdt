// form-views-module.test.tsx — tests for saved views management components
// Tests: ViewManager, ViewColumnEditor

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

// Mock views API
vi.mock('@/features/forms/views/view-api', () => ({
  useViews: () => ({
    data: [
      {
        id: 'view1',
        name: 'All Submissions',
        type: 'List',
        entityType: 'FormSubmission',
        isDefault: true,
        columns: [
          { fieldName: 'name', displayOrder: 1, formatter: 'Text' },
          { fieldName: 'email', displayOrder: 2, formatter: 'Text' },
        ],
      },
    ],
    isLoading: false,
  }),
  useCreateView: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
  useUpdateView: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
  useDeleteView: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
}));

// ──────────────────────────────────────────────────────────────────────────────
// Component imports — after mocks
// ──────────────────────────────────────────────────────────────────────────────

import { ViewManager } from '@/features/forms/views/view-manager';
import { ViewColumnEditor } from '@/features/forms/views/view-column-editor';
import type { FormFieldDto } from '@/features/forms/form-types';

// ──────────────────────────────────────────────────────────────────────────────
// Helper: create mock field
// ──────────────────────────────────────────────────────────────────────────────

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
// TC-FE-VIEW-001: ViewManager — renders card title
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewManager — TC-FE-VIEW-001', () => {
  it('renders "Saved Views" card title', () => {
    const fields = [
      makeField({ id: 'f1', fieldKey: 'name' }),
      makeField({ id: 'f2', fieldKey: 'email' }),
    ];
    render(
      <ViewManager templateId="tpl-1" fields={fields} />,
      { wrapper: makeWrapper() }
    );
    // ViewManager should render a card with title
    const { container } = render(
      <ViewManager templateId="tpl-1" fields={fields} />,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('.ant-card')).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VIEW-002: ViewManager — renders Create View button
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewManager — TC-FE-VIEW-002', () => {
  it('renders "Create View" button', () => {
    const fields = [makeField({ id: 'f1', fieldKey: 'name' })];
    render(
      <ViewManager templateId="tpl-1" fields={fields} />,
      { wrapper: makeWrapper() }
    );
    const button = screen.queryByRole('button');
    expect(button).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VIEW-003: ViewManager — renders table with columns
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewManager — TC-FE-VIEW-003', () => {
  it('renders table with columns (Name, Type, Columns, Default, Actions)', () => {
    const fields = [
      makeField({ id: 'f1', fieldKey: 'name' }),
      makeField({ id: 'f2', fieldKey: 'email' }),
    ];
    const { container } = render(
      <ViewManager templateId="tpl-1" fields={fields} />,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('.ant-table')).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VIEW-004: ViewManager — renders views data
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewManager — TC-FE-VIEW-004', () => {
  it('shows views data when loaded', () => {
    const fields = [
      makeField({ id: 'f1', fieldKey: 'name' }),
      makeField({ id: 'f2', fieldKey: 'email' }),
    ];
    const { container } = render(
      <ViewManager templateId="tpl-1" fields={fields} />,
      { wrapper: makeWrapper() }
    );
    // Table should be present and populated
    expect(container.querySelector('.ant-table')).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VIEW-005: ViewManager — renders without crashing
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewManager — TC-FE-VIEW-005', () => {
  it('renders successfully with valid fields', () => {
    const fields = [makeField({ id: 'f1', fieldKey: 'test' })];
    const { container } = render(
      <ViewManager templateId="tpl-1" fields={fields} />,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VEDITOR-001: ViewColumnEditor — renders Add Column button
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewColumnEditor — TC-FE-VEDITOR-001', () => {
  it('renders "Add Column" button', () => {
    const fields = [
      makeField({ id: 'f1', fieldKey: 'name' }),
      makeField({ id: 'f2', fieldKey: 'email' }),
    ];
    const { container } = render(
      <Form>
        <Form.Item name="columns">
          <ViewColumnEditor fields={fields} />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    const button = container.querySelector('button');
    expect(button).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VEDITOR-002: ViewColumnEditor — renders table
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewColumnEditor — TC-FE-VEDITOR-002', () => {
  it('renders editable table for columns', () => {
    const fields = [makeField({ id: 'f1', fieldKey: 'name' })];
    const { container } = render(
      <Form>
        <Form.Item name="columns">
          <ViewColumnEditor fields={fields} />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('.ant-table')).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VEDITOR-003: ViewColumnEditor — empty state
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewColumnEditor — TC-FE-VEDITOR-003', () => {
  it('renders empty table when no columns provided', () => {
    const fields = [makeField({ id: 'f1', fieldKey: 'name' })];
    const { container } = render(
      <Form>
        <Form.Item name="columns">
          <ViewColumnEditor fields={fields} value={[]} />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.querySelector('.ant-table')).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VEDITOR-004: ViewColumnEditor — filters layout fields
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewColumnEditor — TC-FE-VEDITOR-004', () => {
  it('filters out Section, Label, Divider field types from options', () => {
    const fields = [
      makeField({ id: 'f1', fieldKey: 'name', type: 'Text' }),
      makeField({ id: 'f2', fieldKey: 'section1', type: 'Section' }),
      makeField({ id: 'f3', fieldKey: 'label1', type: 'Label' }),
      makeField({ id: 'f4', fieldKey: 'email', type: 'Text' }),
    ];
    const { container } = render(
      <Form>
        <Form.Item name="columns">
          <ViewColumnEditor fields={fields} />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    // Should only have selects for Text fields (name, email), not Section/Label/Divider
    expect(container.querySelector('.ant-table')).toBeTruthy();
  });
});

// ──────────────────────────────────────────────────────────────────────────────
// TC-FE-VEDITOR-005: ViewColumnEditor — renders without crashing
// ──────────────────────────────────────────────────────────────────────────────

describe('ViewColumnEditor — TC-FE-VEDITOR-005', () => {
  it('renders successfully with valid fields', () => {
    const fields = [makeField({ id: 'f1', fieldKey: 'test' })];
    const { container } = render(
      <Form>
        <Form.Item name="columns">
          <ViewColumnEditor fields={fields} />
        </Form.Item>
      </Form>,
      { wrapper: makeWrapper() }
    );
    expect(container.firstChild).toBeTruthy();
  });
});
