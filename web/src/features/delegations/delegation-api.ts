import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface DelegationDto {
  id: string;
  delegatorId: string;
  delegatorName?: string;
  delegateId: string;
  delegateName?: string;
  validFrom: string;
  validTo: string;
  reason: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateDelegationDto {
  delegatorId: string;
  delegateId: string;
  validFrom: string;
  validTo: string;
  reason?: string;
}

// ─── Query keys ─────────────────────────────────────────────────────────────

export const delegationQueryKeys = {
  list: (activeOnly: boolean) => ['delegations', activeOnly] as const,
};

// ─── Queries ────────────────────────────────────────────────────────────────

/** GET /api/v1/delegations?activeOnly=true — list delegations */
export function useDelegations(activeOnly = true) {
  return useQuery({
    queryKey: delegationQueryKeys.list(activeOnly),
    queryFn: () =>
      apiClient
        .get<DelegationDto[]>('/delegations', { params: { activeOnly } })
        .then((r) => r.data),
  });
}

// ─── Mutations ──────────────────────────────────────────────────────────────

/** POST /api/v1/delegations — create a delegation */
export function useCreateDelegation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateDelegationDto) =>
      apiClient.post<DelegationDto>('/delegations', dto).then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['delegations'] });
    },
  });
}

/** DELETE /api/v1/delegations/{id} — revoke a delegation */
export function useRevokeDelegation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`/delegations/${id}`).then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['delegations'] });
    },
  });
}
