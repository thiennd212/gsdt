import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult } from '@/shared/types/api';
import type {
  PppProjectListItem,
  PppProjectListParams,
  PppProjectDetail,
  CreatePppProjectRequest,
  UpdatePppProjectRequest,
} from './ppp-project-types';

const BASE = '/ppp-projects';

// ─── Query keys ───────────────────────────────────────────────────────────────

export const pppProjectKeys = {
  all: ['ppp-projects'] as const,
  list: (params: PppProjectListParams) => ['ppp-projects', 'list', params] as const,
  detail: (id: string) => ['ppp-projects', 'detail', id] as const,
};

// ─── List query ───────────────────────────────────────────────────────────────

export function usePppProjects(params: PppProjectListParams) {
  return useQuery({
    queryKey: pppProjectKeys.list(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<PppProjectListItem>>(BASE, { params })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });
}

// ─── Detail query ─────────────────────────────────────────────────────────────

export function usePppProject(id: string | undefined) {
  return useQuery({
    queryKey: pppProjectKeys.detail(id ?? ''),
    queryFn: () =>
      apiClient.get<PppProjectDetail>(`${BASE}/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Create ───────────────────────────────────────────────────────────────────

export function useCreatePppProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreatePppProjectRequest) =>
      apiClient.post<{ id: string }>(BASE, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: pppProjectKeys.all }),
  });
}

// ─── Update ───────────────────────────────────────────────────────────────────

export function useUpdatePppProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdatePppProjectRequest) =>
      apiClient.put(`${BASE}/${id}`, body).then((r) => r.data),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: pppProjectKeys.all });
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.id) });
    },
  });
}

// ─── Delete ───────────────────────────────────────────────────────────────────

export function useDeletePppProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`${BASE}/${id}`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: pppProjectKeys.all }),
  });
}

// ─── Decision mutations ───────────────────────────────────────────────────────

export function useAddPppDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/decisions`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeletePppDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, decisionId }: { projectId: string; decisionId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/decisions/${decisionId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Investor selection ───────────────────────────────────────────────────────

export function useUpsertInvestorSelection() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/investor-selection`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Contract info ────────────────────────────────────────────────────────────

export function useUpsertContractInfo() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/contract-info`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Capital plan mutations ───────────────────────────────────────────────────

export function useAddPppCapitalPlan() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/capital-plans`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeletePppCapitalPlan() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, planId }: { projectId: string; planId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/capital-plans/${planId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Design estimate mutations ────────────────────────────────────────────────

export function useAddDesignEstimate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/design-estimates`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

export function useUpdateDesignEstimate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, estimateId, ...body }: { projectId: string; estimateId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/design-estimates/${estimateId}`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDesignEstimate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, estimateId }: { projectId: string; estimateId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/design-estimates/${estimateId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Execution record mutations ───────────────────────────────────────────────

export function useAddPppExecutionRecord() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/execution-records`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Disbursement mutations ───────────────────────────────────────────────────

export function useAddPppDisbursement() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/disbursements`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeletePppDisbursement() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, disbursementId }: { projectId: string; disbursementId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/disbursements/${disbursementId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Revenue report mutations ─────────────────────────────────────────────────

export function useAddRevenueReport() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/revenue-reports`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

export function useUpdateRevenueReport() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, reportId, ...body }: { projectId: string; reportId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/revenue-reports/${reportId}`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteRevenueReport() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, reportId }: { projectId: string; reportId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/revenue-reports/${reportId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Documents ────────────────────────────────────────────────────────────────

export function useAddPppDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/documents`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeletePppDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, documentId }: { projectId: string; documentId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/documents/${documentId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: pppProjectKeys.detail(vars.projectId) }),
  });
}
