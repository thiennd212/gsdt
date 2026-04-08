import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type {
  CatalogItemDto,
  CreateCatalogRequest,
  UpdateCatalogRequest,
  ContractorSelectionPlanDto,
  CreateContractorSelectionPlanRequest,
  UpdateContractorSelectionPlanRequest,
} from './catalog-types';

// ─── Query keys ──────────────────────────────────────────────────────────────

export const catalogKeys = {
  all: ['catalogs'] as const,
  list: (type: string) => ['catalogs', type] as const,
  khlcnt: ['catalogs', 'contractor-selection-plans'] as const,
};

// ─── Generic catalog hooks ──────────────────────────────────────────────────

/** GET /api/v1/masterdata/catalogs/{type}?includeInactive=true */
export function useCatalogItems(type: string) {
  return useQuery({
    queryKey: catalogKeys.list(type),
    queryFn: () =>
      apiClient
        .get<CatalogItemDto[]>(`/masterdata/catalogs/${type}`, {
          params: { includeInactive: true },
        })
        .then((r) => r.data),
    enabled: Boolean(type),
  });
}

/** POST /api/v1/masterdata/catalogs/{type} */
export function useCreateCatalogItem(type: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateCatalogRequest) =>
      apiClient.post(`/masterdata/catalogs/${type}`, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.list(type) }),
  });
}

/** PUT /api/v1/masterdata/catalogs/{type}/{id} */
export function useUpdateCatalogItem(type: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdateCatalogRequest & { id: string }) =>
      apiClient
        .put(`/masterdata/catalogs/${type}/${id}`, body)
        .then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.list(type) }),
  });
}

/** DELETE /api/v1/masterdata/catalogs/{type}/{id} */
export function useDeleteCatalogItem(type: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient
        .delete(`/masterdata/catalogs/${type}/${id}`)
        .then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.list(type) }),
  });
}

// ─── KHLCNT (ContractorSelectionPlan) hooks ─────────────────────────────────

/** GET /api/v1/masterdata/contractor-selection-plans?includeInactive=true */
export function useContractorSelectionPlans() {
  return useQuery({
    queryKey: catalogKeys.khlcnt,
    queryFn: () =>
      apiClient
        .get<ContractorSelectionPlanDto[]>(
          '/masterdata/contractor-selection-plans',
          { params: { includeInactive: true } },
        )
        .then((r) => r.data),
  });
}

/** POST /api/v1/masterdata/contractor-selection-plans */
export function useCreateContractorSelectionPlan() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateContractorSelectionPlanRequest) =>
      apiClient
        .post('/masterdata/contractor-selection-plans', body)
        .then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.khlcnt }),
  });
}

/** PUT /api/v1/masterdata/contractor-selection-plans/{id} */
export function useUpdateContractorSelectionPlan() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      ...body
    }: UpdateContractorSelectionPlanRequest & { id: string }) =>
      apiClient
        .put(`/masterdata/contractor-selection-plans/${id}`, body)
        .then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.khlcnt }),
  });
}

/** DELETE /api/v1/masterdata/contractor-selection-plans/{id} */
export function useDeleteContractorSelectionPlan() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient
        .delete(`/masterdata/contractor-selection-plans/${id}`)
        .then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.khlcnt }),
  });
}
