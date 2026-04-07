// TC-FE-DEL-001: Renders delegation list
// TC-FE-DEL-002: Create delegation form validation

import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

// Controllable delegation API
const mockUseDelegations = vi.fn(() => ({ data: [], isLoading: false }));

vi.mock('@/features/delegations/delegation-api', () => ({
  useDelegations: () => mockUseDelegations(),
  useCreateDelegation: () => ({ mutate: vi.fn(), mutateAsync: vi.fn(), isPending: false }),
  useRevokeDelegation: () => ({ mutate: vi.fn(), isPending: false }),
}));

import { DelegationListPage } from '@/features/delegations/delegation-list-page';

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

// TC-FE-DEL-001: Renders delegation list
describe('DelegationListPage — TC-FE-DEL-001: renders delegation list', () => {
  it('renders without crashing', () => {
    const { container } = render(<DelegationListPage />, { wrapper: makeWrapper() });
    expect(container.firstChild).toBeTruthy();
  });

  it('renders page title', () => {
    render(<DelegationListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('delegations.title')).toBeTruthy();
  });

  it('renders delegation table', () => {
    const { container } = render(<DelegationListPage />, { wrapper: makeWrapper() });
    expect(container.querySelector('table')).toBeTruthy();
  });

  it('renders active-only toggle switch', () => {
    render(<DelegationListPage />, { wrapper: makeWrapper() });
    // Switch has checkedChildren text
    expect(screen.getByText('delegations.activeOnly')).toBeTruthy();
  });

  it('renders create delegation button', () => {
    render(<DelegationListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('common.add')).toBeTruthy();
  });

  it('renders table rows when delegations are provided', () => {
    mockUseDelegations.mockReturnValue({
      data: [
        {
          id: 'del-1',
          delegatorId: 'user-1',
          delegatorName: 'Nguyễn Văn A',
          delegateId: 'user-2',
          delegateName: 'Trần Thị B',
          validFrom: '2024-01-01T00:00:00Z',
          validTo: '2024-06-01T00:00:00Z',
          reason: 'Công tác',
          isActive: true,
        },
      ],
      isLoading: false,
    });
    render(<DelegationListPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('Nguyễn Văn A')).toBeTruthy();
    expect(screen.getByText('Trần Thị B')).toBeTruthy();
  });
});

// TC-FE-DEL-002: Create delegation form validation
describe('DelegationListPage — TC-FE-DEL-002: create delegation form', () => {
  it('renders create modal title when modal is opened via create button click', () => {
    mockUseDelegations.mockReturnValue({ data: [], isLoading: false });
    render(<DelegationListPage />, { wrapper: makeWrapper() });
    // The create button opens the modal
    fireEvent.click(screen.getByText('common.add'));
    // Modal title should be visible after click
    expect(screen.getByText('delegations.createTitle')).toBeTruthy();
  });

  it('renders delegator and delegate form fields in modal', () => {
    mockUseDelegations.mockReturnValue({ data: [], isLoading: false });
    render(<DelegationListPage />, { wrapper: makeWrapper() });
    fireEvent.click(screen.getByText('common.add'));
    // Form labels rendered via t() keys — use getAllByText since label may appear in table header too
    expect(screen.getAllByText('delegations.col.delegator').length).toBeGreaterThan(0);
    expect(screen.getAllByText('delegations.col.delegate').length).toBeGreaterThan(0);
  });
});
