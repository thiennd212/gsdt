import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { KpiStatCards } from '@/features/dashboard/kpi-stat-cards';
import type { KpiDashboardDto } from '@/features/dashboard/dashboard-types';

// t mock returns key as-is — titles render as i18n keys
vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const mockKpiData: KpiDashboardDto = {
  totalCases: 150,
  openCases: 42,
  closedCases: 108,
  averageResolutionDays: 3.75,
  casesByType: { Application: 60, Complaint: 30, Request: 40, Report: 20 },
  casesByStatus: { Draft: 10, Submitted: 20, Approved: 108 },
  casesByPriority: { Low: 50, Medium: 60, High: 30, Critical: 10 },
  topAssignees: [],
  monthlyTrend: [],
};

describe('KpiStatCards — renders all 4 stat cards', () => {
  it('renders total cases card with correct value', () => {
    render(<KpiStatCards data={mockKpiData} />);
    expect(screen.getByText('page.dashboard.kpi.totalCases')).toBeTruthy();
    expect(screen.getByText('150')).toBeTruthy();
  });

  it('renders open cases card with correct value', () => {
    render(<KpiStatCards data={mockKpiData} />);
    expect(screen.getByText('page.dashboard.kpi.openCases')).toBeTruthy();
    expect(screen.getByText('42')).toBeTruthy();
  });

  it('renders closed cases card with correct value', () => {
    render(<KpiStatCards data={mockKpiData} />);
    expect(screen.getByText('page.dashboard.kpi.closedCases')).toBeTruthy();
    expect(screen.getByText('108')).toBeTruthy();
  });

  it('renders average resolution days card', () => {
    render(<KpiStatCards data={mockKpiData} />);
    expect(screen.getByText('page.dashboard.kpi.avgResolution')).toBeTruthy();
  });
});

describe('KpiStatCards — loading state', () => {
  it('renders without crashing when loading=true', () => {
    const { container } = render(<KpiStatCards data={mockKpiData} loading={true} />);
    expect(container.firstChild).toBeTruthy();
  });
});

describe('KpiStatCards — zero values', () => {
  it('renders correctly with all-zero KPI data', () => {
    const zeroData: KpiDashboardDto = {
      ...mockKpiData,
      totalCases: 0,
      openCases: 0,
      closedCases: 0,
      averageResolutionDays: 0,
    };
    render(<KpiStatCards data={zeroData} />);
    expect(screen.getByText('page.dashboard.kpi.totalCases')).toBeTruthy();
    expect(screen.getByText('page.dashboard.kpi.openCases')).toBeTruthy();
  });
});
