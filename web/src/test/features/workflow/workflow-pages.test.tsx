// TC-FE-WF-001: Inbox renders pending tasks
// TC-FE-WF-002: Inbox empty state
// TC-FE-WF-003: Admin page renders definitions

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
  useParams: () => ({}),
  Link: ({ children }: { children: ReactNode }) => children,
}));

// Mock CaseStatusTag / CasePriorityTag used inside inbox
vi.mock('@/features/cases/case-status-tag', () => ({
  CaseStatusTag: ({ status }: { status: string }) =>
    createElement('span', { 'data-testid': `status-${status}` }, status),
  CasePriorityTag: ({ priority }: { priority: string }) =>
    createElement('span', { 'data-testid': `priority-${priority}` }, priority),
}));

// Controllable inbox API
const mockUseWorkflowInbox = vi.fn(() => ({ data: { items: [] }, isFetching: false }));

vi.mock('@/features/inbox/inbox-api', () => ({
  useWorkflowInbox: () => mockUseWorkflowInbox(),
}));

// Controllable workflow definitions API
const mockUseWorkflowDefinitions = vi.fn(() => ({
  data: { items: [], totalCount: 0 },
  isLoading: false,
}));

vi.mock('@/features/workflow/workflow-api', () => ({
  useWorkflowDefinitions: () => mockUseWorkflowDefinitions(),
  useCreateWorkflow: () => ({ mutateAsync: vi.fn(), isPending: false }),
  useDeleteWorkflowDefinition: () => ({ mutateAsync: vi.fn(), isPending: false }),
}));

import { WorkflowInboxPage } from '@/features/inbox/inbox-page';
import { WorkflowAdminPage } from '@/features/workflow/workflow-admin-page';

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

// TC-FE-WF-001: Inbox renders pending tasks
describe('WorkflowInboxPage — TC-FE-WF-001: renders pending tasks', () => {
  it('renders without crashing', () => {
    mockUseWorkflowInbox.mockReturnValue({ data: { items: [] }, isFetching: false });
    const { container } = render(<WorkflowInboxPage />, { wrapper: makeWrapper() });
    expect(container.firstChild).toBeTruthy();
  });

  it('renders inbox page title', () => {
    render(<WorkflowInboxPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.inbox.title')).toBeTruthy();
  });

  it('renders pending case rows in inbox table', () => {
    mockUseWorkflowInbox.mockReturnValue({
      data: {
        items: [
          {
            id: 'case-1',
            caseNumber: 'HS-001',
            title: 'Hồ sơ xin cấp phép xây dựng',
            status: 'PendingApproval',
            priority: 'High',
            assignedAtUtc: '2024-06-01T09:00:00Z',
          },
          {
            id: 'case-2',
            caseNumber: 'HS-002',
            title: 'Hồ sơ đăng ký kinh doanh',
            status: 'PendingReview',
            priority: 'Medium',
            assignedAtUtc: '2024-06-02T10:00:00Z',
          },
        ],
      },
      isFetching: false,
    });
    render(<WorkflowInboxPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('HS-001')).toBeTruthy();
    expect(screen.getByText('Hồ sơ xin cấp phép xây dựng')).toBeTruthy();
    expect(screen.getByText('HS-002')).toBeTruthy();
  });

  it('renders inbox table element', () => {
    mockUseWorkflowInbox.mockReturnValue({ data: { items: [] }, isFetching: false });
    const { container } = render(<WorkflowInboxPage />, { wrapper: makeWrapper() });
    expect(container.querySelector('table')).toBeTruthy();
  });
});

// TC-FE-WF-002: Inbox empty state
describe('WorkflowInboxPage — TC-FE-WF-002: empty state', () => {
  it('renders empty locale text when inbox has no items', () => {
    mockUseWorkflowInbox.mockReturnValue({ data: { items: [] }, isFetching: false });
    render(<WorkflowInboxPage />, { wrapper: makeWrapper() });
    // Ant Design Table renders locale.emptyText when dataSource is empty
    expect(screen.getByText('page.inbox.empty')).toBeTruthy();
  });

  it('renders empty locale text (no data rows) when items array is empty', () => {
    mockUseWorkflowInbox.mockReturnValue({ data: { items: [] }, isFetching: false });
    render(<WorkflowInboxPage />, { wrapper: makeWrapper() });
    // Ant Design renders emptyText when dataSource is empty
    expect(screen.getByText('page.inbox.empty')).toBeTruthy();
  });

  it('renders loading state while fetching', () => {
    mockUseWorkflowInbox.mockReturnValue({ data: undefined, isFetching: true });
    const { container } = render(<WorkflowInboxPage />, { wrapper: makeWrapper() });
    expect(container.querySelector('.ant-spin')).toBeTruthy();
  });
});

// TC-FE-WF-003: Admin page renders definitions
describe('WorkflowAdminPage — TC-FE-WF-003: renders definitions', () => {
  it('renders without crashing', () => {
    mockUseWorkflowDefinitions.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });
    const { container } = render(<WorkflowAdminPage />, { wrapper: makeWrapper() });
    expect(container.firstChild).toBeTruthy();
  });

  it('renders workflow admin page title', () => {
    render(<WorkflowAdminPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('workflow.title')).toBeTruthy();
  });

  it('renders create workflow button', () => {
    render(<WorkflowAdminPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('workflow.createTitle')).toBeTruthy();
  });

  it('renders definitions table', () => {
    const { container } = render(<WorkflowAdminPage />, { wrapper: makeWrapper() });
    expect(container.querySelector('table')).toBeTruthy();
  });

  it('renders workflow definition rows when data is provided', () => {
    mockUseWorkflowDefinitions.mockReturnValue({
      data: {
        items: [
          {
            id: 'wf-1',
            name: 'Case Approval Workflow',
            entityType: 'Case',
            stateCount: 5,
            transitionCount: 8,
            createdAt: '2024-01-01T00:00:00Z',
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    });
    render(<WorkflowAdminPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('Case Approval Workflow')).toBeTruthy();
    // stateCount and transitionCount rendered in table cells
    expect(screen.getByText('5')).toBeTruthy();
  });

  it('opens create workflow modal when create button is clicked', () => {
    mockUseWorkflowDefinitions.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });
    render(<WorkflowAdminPage />, { wrapper: makeWrapper() });
    fireEvent.click(screen.getByText('workflow.createTitle'));
    // After click the modal form is open — form label appears in addition to table header
    // workflow.col.name appears as table <th> + modal form <label>, so ≥2 elements
    expect(screen.getAllByText('workflow.col.name').length).toBeGreaterThanOrEqual(2);
    // modal form label for entityType is rendered once inside the modal
    expect(screen.getByText('workflow.col.entityType')).toBeTruthy();
  });
});
