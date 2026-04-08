import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult } from '@/shared/types/api';
import type {
  DnnnProjectListItem,
  DnnnProjectListParams,
  DnnnProjectDetail,
  CreateDnnnProjectRequest,
  UpdateDnnnProjectRequest,
} from './dnnn-project-types';

const BASE = '/dnnn-projects';

// ─── Query keys ───────────────────────────────────────────────────────────────

export const dnnnProjectKeys = {
  all: ['dnnn-projects'] as const,
  list: (params: DnnnProjectListParams) => ['dnnn-projects', 'list', params] as const,
  detail: (id: string) => ['dnnn-projects', 'detail', id] as const,
};

// ─── List query ───────────────────────────────────────────────────────────────

export function useDnnnProjects(params: DnnnProjectListParams) {
  return useQuery({
    queryKey: dnnnProjectKeys.list(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<DnnnProjectListItem>>(BASE, { params })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });
}

// ─── Detail query ─────────────────────────────────────────────────────────────

export function useDnnnProject(id: string | undefined) {
  return useQuery({
    queryKey: dnnnProjectKeys.detail(id ?? ''),
    queryFn: () =>
      apiClient.get<DnnnProjectDetail>(`${BASE}/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Create ───────────────────────────────────────────────────────────────────

export function useCreateDnnnProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateDnnnProjectRequest) =>
      apiClient.post<{ id: string }>(BASE, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: dnnnProjectKeys.all }),
  });
}

// ─── Update ───────────────────────────────────────────────────────────────────

export function useUpdateDnnnProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdateDnnnProjectRequest) =>
      apiClient.put(`${BASE}/${id}`, body).then((r) => r.data),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.all });
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.id) });
    },
  });
}

// ─── Delete ───────────────────────────────────────────────────────────────────

export function useDeleteDnnnProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`${BASE}/${id}`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: dnnnProjectKeys.all }),
  });
}

// ─── Decision mutations ───────────────────────────────────────────────────────

export function useAddDnnnDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/decisions`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDnnnDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, decisionId }: { projectId: string; decisionId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/decisions/${decisionId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Certificate mutations ────────────────────────────────────────────────────

export function useAddDnnnCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/certificates`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

export function useUpdateDnnnCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, certificateId, ...body }: { projectId: string; certificateId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/certificates/${certificateId}`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDnnnCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, certificateId }: { projectId: string; certificateId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/certificates/${certificateId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Investor selection ───────────────────────────────────────────────────────

export function useUpsertDnnnInvestorSelection() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/investor-selection`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Design estimate mutations ────────────────────────────────────────────────

export function useAddDnnnDesignEstimate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/design-estimates`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

export function useUpdateDnnnDesignEstimate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, estimateId, ...body }: { projectId: string; estimateId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/design-estimates/${estimateId}`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDnnnDesignEstimate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, estimateId }: { projectId: string; estimateId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/design-estimates/${estimateId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Bid package mutations ────────────────────────────────────────────────────

export function useAddDnnnBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/bid-packages`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDnnnBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, bidPackageId }: { projectId: string; bidPackageId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/bid-packages/${bidPackageId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Document mutations ───────────────────────────────────────────────────────

export function useAddDnnnDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/documents`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDnnnDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, documentId }: { projectId: string; documentId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/documents/${documentId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Location mutations ───────────────────────────────────────────────────────

export function useAddDnnnLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/locations`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDnnnLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, locationId }: { projectId: string; locationId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/locations/${locationId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: dnnnProjectKeys.detail(vars.projectId) }),
  });
}
