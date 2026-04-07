// Roles API hook — wraps GET /api/v1/admin/roles (Identity module)

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/core/api';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface RoleDefinitionDto {
  id: string;
  name: string;
  description?: string;
}

// ─── Query keys ───────────────────────────────────────────────────────────────

export const rolesQueryKeys = {
  all: ['roles'] as const,
  list: ['roles', 'list'] as const,
};

// ─── Query ────────────────────────────────────────────────────────────────────

/** GET /api/v1/admin/roles — static role catalogue (Admin-only endpoint) */
export function useRoles() {
  return useQuery({
    queryKey: rolesQueryKeys.list,
    queryFn: () =>
      apiClient.get<RoleDefinitionDto[]>('/admin/roles').then((r) => r.data),
    // Roles are static — cache for 10 minutes, no background refetch needed
    staleTime: 10 * 60 * 1000,
  });
}
