import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

// Mock i18next — return key as translation value so assertions use translation keys
vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

// Mock TanStack Router
vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
  useParams: () => ({}),
  Link: ({ children }: { children: ReactNode }) => children,
}));

// Mock server pagination hook
vi.mock('@/core/hooks/use-server-pagination', () => ({
  useServerPagination: () => ({
    antPagination: { current: 1, pageSize: 20 },
    toQueryParams: () => ({ pageNumber: 1, pageSize: 20 }),
  }),
}));

// Mock case API hooks — controllable return values
const mockUseCases = vi.fn(() => ({ data: { items: [], totalCount: 0 }, isFetching: false }));
const mockUseApproveCase = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));
const mockUseRejectCase = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));

vi.mock('@/features/cases/case-api', () => ({
  useCases: () => mockUseCases(),
  useApproveCase: () => mockUseApproveCase(),
  useRejectCase: () => mockUseRejectCase(),
  useCreateCase: () => ({ mutateAsync: vi.fn(), isPending: false }),
}));

import { CaseListPage } from '@/features/cases/case-list-page';

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

describe('CaseListPage — basic render', () => {
  it('renders without crashing', () => {
    const { container } = render(<CaseListPage />, { wrapper: makeWrapper() });
    expect(container.firstChild).toBeTruthy();
  });

  it('renders page title translation key', () => {
    render(<CaseListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.cases.title')).toBeTruthy();
  });

  it('renders create button', () => {
    render(<CaseListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.cases.createBtn')).toBeTruthy();
  });

  it('renders table with no rows when data is empty', () => {
    mockUseCases.mockReturnValue({ data: { items: [], totalCount: 0 }, isFetching: false });
    const { container } = render(<CaseListPage />, { wrapper: makeWrapper() });
    // Ant Design Table renders a <table> element
    expect(container.querySelector('table')).toBeTruthy();
  });

  it('renders table rows when data is provided', () => {
    mockUseCases.mockReturnValue({
      data: {
        items: [
          { id: 'case-1', caseNumber: 'HS-001', title: 'Test Case', type: 'Application', status: 'Draft', priority: 'Medium', createdAt: '2024-01-01T00:00:00Z' },
        ],
        totalCount: 1,
      },
      isFetching: false,
    });
    render(<CaseListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('HS-001')).toBeTruthy();
    expect(screen.getByText('Test Case')).toBeTruthy();
  });
});
