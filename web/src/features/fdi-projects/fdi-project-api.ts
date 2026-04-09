import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult } from '@/shared/types/api';
import type {
  FdiProjectListItem,
  FdiProjectListParams,
  FdiProjectDetail,
  CreateFdiProjectRequest,
  UpdateFdiProjectRequest,
} from './fdi-project-types';

const BASE = '/fdi-projects';

// ─── Query keys ───────────────────────────────────────────────────────────────

export const fdiProjectKeys = {
  all: ['fdi-projects'] as const,
  list: (params: FdiProjectListParams) => ['fdi-projects', 'list', params] as const,
  detail: (id: string) => ['fdi-projects', 'detail', id] as const,
};

// ─── List query ───────────────────────────────────────────────────────────────

export function useFdiProjects(params: FdiProjectListParams) {
  return useQuery({
    queryKey: fdiProjectKeys.list(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<FdiProjectListItem>>(BASE, { params })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });
}

// ─── Detail query ─────────────────────────────────────────────────────────────

export function useFdiProject(id: string | undefined) {
  return useQuery({
    queryKey: fdiProjectKeys.detail(id ?? ''),
    queryFn: () =>
      apiClient.get<FdiProjectDetail>(`${BASE}/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Create ───────────────────────────────────────────────────────────────────

export function useCreateFdiProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateFdiProjectRequest) =>
      apiClient.post<{ id: string }>(BASE, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: fdiProjectKeys.all }),
  });
}

// ─── Update ───────────────────────────────────────────────────────────────────

export function useUpdateFdiProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdateFdiProjectRequest) =>
      apiClient.put(`${BASE}/${id}`, body).then((r) => r.data),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: fdiProjectKeys.all });
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.id) });
    },
  });
}

// ─── Delete ───────────────────────────────────────────────────────────────────

export function useDeleteFdiProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`${BASE}/${id}`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: fdiProjectKeys.all }),
  });
}

// ─── Decision mutations ───────────────────────────────────────────────────────

export function useAddFdiDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/decisions`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteFdiDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, decisionId }: { projectId: string; decisionId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/decisions/${decisionId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Certificate mutations ────────────────────────────────────────────────────

export function useAddFdiCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/certificates`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

export function useUpdateFdiCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, certificateId, ...body }: { projectId: string; certificateId: string } & Record<string, unknown>) =>
      apiClient.put(`${BASE}/${projectId}/certificates/${certificateId}`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteFdiCertificate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, certificateId }: { projectId: string; certificateId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/certificates/${certificateId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Bid package mutations ────────────────────────────────────────────────────

export function useAddFdiBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/bid-packages`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteFdiBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, bidPackageId }: { projectId: string; bidPackageId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/bid-packages/${bidPackageId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Document mutations ───────────────────────────────────────────────────────

export function useAddFdiDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/documents`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteFdiDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, documentId }: { projectId: string; documentId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/documents/${documentId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Location mutations ───────────────────────────────────────────────────────

export function useAddFdiLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/locations`, body).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteFdiLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, locationId }: { projectId: string; locationId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/locations/${locationId}`).then((r) => r.data),
    onSuccess: (_, vars) =>
      qc.invalidateQueries({ queryKey: fdiProjectKeys.detail(vars.projectId) }),
  });
}
