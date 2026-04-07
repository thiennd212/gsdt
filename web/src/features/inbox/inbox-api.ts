import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult } from '@/shared/types/api';
import type { CaseDto } from '@/features/cases/case-types';

// ─── Query keys ──────────────────────────────────────────────────────────────

export const inboxQueryKeys = {
  all: ['inbox'] as const,
  list: () => ['inbox', 'list'] as const,
};

// ─── Hooks ───────────────────────────────────────────────────────────────────

/**
 * Fetch workflow inbox — cases assigned to current user pending action.
 * GET /api/v1/cases?status=Submitted,UnderReview,ReturnedForRevision (filtered server-side by assignee)
 */
export function useWorkflowInbox() {
  return useQuery({
    queryKey: inboxQueryKeys.list(),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<CaseDto>>('/cases', {
          params: { status: 'Submitted,UnderReview,ReturnedForRevision', pageSize: 50 },
        })
        .then((r) => r.data),
    refetchInterval: 60_000, // refresh every minute
  });
}
