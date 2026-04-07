import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { KpiDashboardDto } from './dashboard-types';

// ─── Query keys ───────────────────────────────────────────────────────────────

export const dashboardQueryKeys = {
  kpi: ['dashboard', 'kpi'] as const,
};

// ─── Queries ──────────────────────────────────────────────────────────────────

/** GET /api/v1/reports/dashboard — cached 5 minutes on backend; auto-refresh here too */
export function useDashboardKpi() {
  return useQuery({
    queryKey: dashboardQueryKeys.kpi,
    queryFn: () =>
      apiClient.get<KpiDashboardDto>('/reports/dashboard').then((r) => r.data),
    // Backend caches 5 min; match frontend refresh interval
    refetchInterval: 300_000,
    staleTime: 60_000,
    retry: false, // 403 for non-admin is expected — don't retry
  });
}
