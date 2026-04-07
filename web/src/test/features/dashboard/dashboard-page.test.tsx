import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock('@/features/auth', () => ({
  useAuth: () => ({ user: { profile: { name: 'Test User' } }, login: vi.fn(), logout: vi.fn(), isLoading: false }),
}));

// Mock lazy-loaded chart components to avoid echarts in jsdom
vi.mock('@/features/dashboard/cases-by-status-chart', () => ({
  CasesByStatusChart: () => <div data-testid="status-chart" />,
}));
vi.mock('@/features/dashboard/cases-by-type-chart', () => ({
  CasesByTypeChart: () => <div data-testid="type-chart" />,
}));
vi.mock('@/features/dashboard/monthly-trend-chart', () => ({
  MonthlyTrendChart: () => <div data-testid="trend-chart" />,
}));
vi.mock('@/features/dashboard/top-assignees-table', () => ({
  TopAssigneesTable: () => <div data-testid="assignees-table" />,
}));
vi.mock('@/features/dashboard/kpi-stat-cards', () => ({
  KpiStatCards: ({ loading }: { loading?: boolean }) => (
    <div data-testid="kpi-stat-cards">{loading ? 'loading' : 'kpi-cards'}</div>
  ),
}));

const MOCK_KPI = {
  totalCases: 100,
  openCases: 30,
  closedCases: 70,
  averageResolutionDays: 4,
  casesByType: {},
  casesByStatus: {},
  casesByPriority: {},
  topAssignees: [],
  monthlyTrend: [],
};

const mockUseDashboardKpi = vi.fn(() => ({
  data: MOCK_KPI,
  isFetching: false,
  isLoading: false,
  isError: false,
  refetch: vi.fn(),
}));

const mockUseAnnouncements = vi.fn(() => ({ data: [] }));

vi.mock('@/features/dashboard/dashboard-api', () => ({
  useDashboardKpi: () => mockUseDashboardKpi(),
}));

vi.mock('@/features/system-params/system-params-api', () => ({
  useAnnouncements: () => mockUseAnnouncements(),
}));

import { DashboardPage } from '@/features/dashboard/dashboard-page';

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

describe('DashboardPage — loaded state', () => {
  it('renders without crashing', () => {
    const { container } = render(<DashboardPage />, { wrapper: makeWrapper() });
    expect(container.firstChild).toBeTruthy();
  });

  it('renders page title', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() });
    // Greeting uses t() with defaultValue, returns key when mocked
    expect(screen.getByText(/page\.dashboard\.greeting|Xin chào/)).toBeTruthy();
  });

  it('renders KPI stat cards', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() });
    expect(screen.getByTestId('kpi-stat-cards')).toBeTruthy();
  });

  it('renders refresh button', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.dashboard.refresh')).toBeTruthy();
  });
});

describe('DashboardPage — loading state', () => {
  it('renders loading spinner when isLoading=true', () => {
    mockUseDashboardKpi.mockReturnValue({
      data: undefined,
      isFetching: true,
      isLoading: true,
      isError: false,
      refetch: vi.fn(),
    });
    const { container } = render(<DashboardPage />, { wrapper: makeWrapper() });
    // Ant Design Spin renders aria-busy="true" on the spinner wrapper
    expect(container.querySelector('[aria-busy="true"]')).toBeTruthy();
  });
});

describe('DashboardPage — error state', () => {
  it('renders error alert when isError=true', () => {
    mockUseDashboardKpi.mockReturnValue({
      data: undefined,
      isFetching: false,
      isLoading: false,
      isError: true,
      refetch: vi.fn(),
    });
    render(<DashboardPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('page.dashboard.errorMessage')).toBeTruthy();
  });
});

describe('DashboardPage — announcement banners', () => {
  it('renders active announcement banner', () => {
    mockUseDashboardKpi.mockReturnValue({
      data: MOCK_KPI,
      isFetching: false,
      isLoading: false,
      isError: false,
      refetch: vi.fn(),
    });
    mockUseAnnouncements.mockReturnValue({
      data: [
        {
          id: 'ann-1',
          title: 'Thông báo quan trọng',
          content: 'Nội dung thông báo',
          status: 'Active',
          startDate: null,
          endDate: null,
        },
      ],
    });
    render(<DashboardPage />, { wrapper: makeWrapper() });
    expect(screen.getByText('Thông báo quan trọng')).toBeTruthy();
  });
});
