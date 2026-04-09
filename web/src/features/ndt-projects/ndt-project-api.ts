import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult } from '@/shared/types/api';
import type {
  NdtProjectListItem,
  NdtProjectListParams,
  NdtProjectDetail,
  CreateNdtProjectRequest,
  UpdateNdtProjectRequest,
} from './ndt-project-types';

const BASE = '/ndt-projects';

// ─── Query keys ───────────────────────────────────────────────────────────────

export const ndtProjectKeys = {
  all: ['ndt-projects'] as const,
  list: (params: NdtProjectListParams) => ['ndt-projects', 'list', params] as const,
  detail: (id: string) => ['ndt-projects', 'detail', id] as const,
};

// ─── List query ───────────────────────────────────────────────────────────────

export function useNdtProjects(params: NdtProjectListParams) {
  return useQuery({
    queryKey: ndtProjectKeys.list(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<NdtProjectListItem>>(BASE, { params })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });
}

// ─── Detail query ─────────────────────────────────────────────────────────────

export function useNdtProject(id: string | undefined) {
  return useQuery({
    queryKey: ndtProjectKeys.detail(id ?? ''),
    queryFn: () =>
      apiClient.get<NdtProjectDetail>(`${BASE}/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Create ───────────────────────────────────────────────────────────────────

export function useCreateNdtProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateNdtProjectRequest) =>
      apiClient.post<{ id: string }>(BASE, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ndtProjectKeys.all }),
  });
}

// ─── Update ───────────────────────────────────────────────────────────────────

export function useUpdateNdtProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdateNdtProjectRequest) =>
      apiClient.put(`${BASE}/${id}`, body).then((r) => r.data),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: ndtProjectKeys.all });
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.id) });
    },
  });
}

// ─── Delete ───────────────────────────────────────────────────────────────────

export function useDeleteNdtProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`${BASE}/${id}`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ndtProjectKeys.all }),
  });
}

// ─── Decision mutations ───────────────────────────────────────────────────────

export function useAddNdtDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/decisions`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteNdtDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, decisionId }: { projectId: string; decisionId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/decisions/${decisionId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Certificate mutations ────────────────────────────────────────────────────

export function useAddNdtCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/certificates`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

export function useUpdateNdtCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, certificateId, ...body }: { projectId: string; certificateId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/certificates/${certificateId}`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteNdtCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, certificateId }: { projectId: string; certificateId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/certificates/${certificateId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Bid package mutations ────────────────────────────────────────────────────

export function useAddNdtBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/bid-packages`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteNdtBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, bidPackageId }: { projectId: string; bidPackageId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/bid-packages/${bidPackageId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Document mutations ───────────────────────────────────────────────────────

export function useAddNdtDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/documents`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteNdtDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, documentId }: { projectId: string; documentId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/documents/${documentId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Location mutations ───────────────────────────────────────────────────────

export function useAddNdtLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/locations`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteNdtLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, locationId }: { projectId: string; locationId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/locations/${locationId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: ndtProjectKeys.detail(vars.projectId) }),
  });
}
