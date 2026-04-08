import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type {
  CatalogItemDto,
  CreateCatalogRequest,
  UpdateCatalogRequest,
  ContractorSelectionPlanDto,
  CreateContractorSelectionPlanRequest,
  UpdateContractorSelectionPlanRequest,
  GovernmentAgencyDto,
  GovernmentAgencyTreeNode,
  CreateGovernmentAgencyRequest,
  UpdateGovernmentAgencyRequest,
  InvestorDto,
  CreateInvestorRequest,
  UpdateInvestorRequest,
} from './catalog-types';

// ─── Query keys ──────────────────────────────────────────────────────────────

export const catalogKeys = {
  all: ['catalogs'] as const,
  list: (type: string) => ['catalogs', type] as const,
  khlcnt: ['catalogs', 'contractor-selection-plans'] as const,
  governmentAgencies: ['catalogs', 'government-agencies'] as const,
  governmentAgencyTree: ['catalogs', 'government-agencies', 'tree'] as const,
  investors: ['catalogs', 'investors'] as const,
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

// ─── GovernmentAgency hooks ──────────────────────────────────────────────────

/** GET /api/v1/masterdata/government-agencies?includeInactive=true */
export function useGovernmentAgencies(includeInactive = true) {
  return useQuery({
    queryKey: catalogKeys.governmentAgencies,
    queryFn: () =>
      apiClient
        .get<GovernmentAgencyDto[]>('/masterdata/government-agencies', {
          params: { includeInactive },
        })
        .then((r) => r.data),
  });
}

/** GET /api/v1/masterdata/government-agencies/tree */
export function useGovernmentAgencyTree() {
  return useQuery({
    queryKey: catalogKeys.governmentAgencyTree,
    queryFn: () =>
      apiClient
        .get<GovernmentAgencyTreeNode[]>('/masterdata/government-agencies/tree')
        .then((r) => r.data),
  });
}

/** POST /api/v1/masterdata/government-agencies */
export function useCreateGovernmentAgency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateGovernmentAgencyRequest) =>
      apiClient.post('/masterdata/government-agencies', body).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: catalogKeys.governmentAgencies });
      qc.invalidateQueries({ queryKey: catalogKeys.governmentAgencyTree });
    },
  });
}

/** PUT /api/v1/masterdata/government-agencies/{id} */
export function useUpdateGovernmentAgency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdateGovernmentAgencyRequest & { id: string }) =>
      apiClient
        .put(`/masterdata/government-agencies/${id}`, body)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: catalogKeys.governmentAgencies });
      qc.invalidateQueries({ queryKey: catalogKeys.governmentAgencyTree });
    },
  });
}

/** DELETE /api/v1/masterdata/government-agencies/{id} */
export function useDeleteGovernmentAgency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`/masterdata/government-agencies/${id}`).then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: catalogKeys.governmentAgencies });
      qc.invalidateQueries({ queryKey: catalogKeys.governmentAgencyTree });
    },
  });
}

// ─── Investor hooks ──────────────────────────────────────────────────────────

/** GET /api/v1/masterdata/investors?investorType=... */
export function useInvestors(investorType?: string) {
  return useQuery({
    queryKey: investorType
      ? [...catalogKeys.investors, investorType]
      : catalogKeys.investors,
    queryFn: () =>
      apiClient
        .get<InvestorDto[]>('/masterdata/investors', {
          params: { includeInactive: true, ...(investorType ? { investorType } : {}) },
        })
        .then((r) => r.data),
  });
}

/** POST /api/v1/masterdata/investors */
export function useCreateInvestor() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateInvestorRequest) =>
      apiClient.post('/masterdata/investors', body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.investors }),
  });
}

/** PUT /api/v1/masterdata/investors/{id} */
export function useUpdateInvestor() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdateInvestorRequest & { id: string }) =>
      apiClient
        .put(`/masterdata/investors/${id}`, body)
        .then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.investors }),
  });
}

/** DELETE /api/v1/masterdata/investors/{id} */
export function useDeleteInvestor() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`/masterdata/investors/${id}`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: catalogKeys.investors }),
  });
}
