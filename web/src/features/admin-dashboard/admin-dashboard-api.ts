import { useQueries, useQuery } from '@tanstack/react-query';
import axios from 'axios';
import { apiClient } from '@/core/api/api-client';
import type {
  AdminDashboardStats,
  AdminActivityEntry,
  HealthApiResponse,
  PagedApiResponse,
  JobItem,
} from './admin-dashboard-types';

// ─── Query keys ────────────────────────────────────────────────────────────────

export const adminDashboardQueryKeys = {
  stats: ['admin-dashboard', 'stats'] as const,
  activity: ['admin-dashboard', 'activity'] as const,
};

// ─── Helpers ───────────────────────────────────────────────────────────────────

function normalizeHealthStatus(raw: string): AdminDashboardStats['healthStatus'] {
  const lower = raw.toLowerCase();
  if (lower === 'healthy') return 'healthy';
  if (lower === 'degraded') return 'degraded';
  return 'unhealthy';
}

// ─── Hooks ─────────────────────────────────────────────────────────────────────

/**
 * useAdminDashboardStats — fires 4 parallel requests then merges into one shape.
 * Returns { data, isLoading, isError, refetch }.
 */
export function useAdminDashboardStats() {
  const INTERVAL = 60_000;

  const results = useQueries({
    queries: [
      {
        queryKey: ['admin-dashboard', 'health'],
        queryFn: () =>
          // Health endpoint is middleware-level (not /api/v1 prefix), proxied via Vite /health route
          axios.get<HealthApiResponse>('/health/ready').then((r) => r.data),
        refetchInterval: INTERVAL,
        retry: false,
      },
      {
        queryKey: ['admin-dashboard', 'users'],
        queryFn: () =>
          apiClient
            .get<PagedApiResponse<unknown>>('/admin/users', { params: { pageSize: 1 } })
            .then((r) => r.data),
        refetchInterval: INTERVAL,
        retry: false,
      },
      {
        queryKey: ['admin-dashboard', 'sessions'],
        queryFn: () =>
          // SessionAdminController GET endpoint is /api/v1/admin/sessions/active
          apiClient
            .get<PagedApiResponse<unknown>>('/admin/sessions/active', { params: { pageSize: 1 } })
            .then((r) => r.data),
        refetchInterval: INTERVAL,
        retry: false,
      },
      {
        queryKey: ['admin-dashboard', 'jobs'],
        queryFn: () =>
          apiClient
            .get<PagedApiResponse<JobItem>>('/admin/jobs', { params: { pageSize: 100 } })
            .then((r) => r.data),
        refetchInterval: INTERVAL,
        retry: false,
      },
    ],
  });

  const [healthQ, usersQ, sessionsQ, jobsQ] = results;

  const isLoading = results.some((r) => r.isLoading);
  const isError = results.every((r) => r.isError);

  const data: AdminDashboardStats | undefined =
    !isLoading
      ? {
          healthStatus: healthQ.data
            ? normalizeHealthStatus(healthQ.data.status)
            : 'unhealthy',
          healthChecks: healthQ.data?.checks ?? [],
          totalUsers: usersQ.data?.totalCount ?? 0,
          activeSessions: sessionsQ.data?.totalCount ?? 0,
          pendingJobs: jobsQ.data?.items.filter((j) => j.status === 'Pending').length ?? 0,
          failedJobs: jobsQ.data?.items.filter((j) => j.status === 'Failed').length ?? 0,
        }
      : undefined;

  function refetch() {
    results.forEach((r) => r.refetch());
  }

  return { data, isLoading, isError, refetch };
}

/**
 * useRecentAdminActivity — fetches last 10 audit log entries sorted newest first.
 * Maps BE AuditLogDto fields to AdminActivityEntry shape.
 */
export function useRecentAdminActivity() {
  return useQuery({
    queryKey: adminDashboardQueryKeys.activity,
    queryFn: () =>
      apiClient
        .get<PagedApiResponse<AdminActivityEntry>>('/audit/logs', {
          params: { pageSize: 10 },
        })
        .then((r) => r.data?.items ?? []),
    refetchInterval: 60_000,
    retry: false,
  });
}
