import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult } from '@/shared/types/api';
import type {
  OdaProjectListItem,
  OdaProjectListParams,
  OdaProjectDetail,
  CreateOdaProjectRequest,
  UpdateOdaProjectRequest,
} from './oda-project-types';

const BASE = '/oda-projects';

// ─── Query keys ──────────────────────────────────────────────────────────────

export const odaProjectKeys = {
  all: ['oda-projects'] as const,
  list: (params: OdaProjectListParams) => ['oda-projects', 'list', params] as const,
  detail: (id: string) => ['oda-projects', 'detail', id] as const,
};

// ─── List ────────────────────────────────────────────────────────────────────

export function useOdaProjects(params: OdaProjectListParams) {
  return useQuery({
    queryKey: odaProjectKeys.list(params),
    queryFn: () =>
      apiClient.get<PaginatedResult<OdaProjectListItem>>(BASE, { params }).then((r) => r.data),
    placeholderData: (prev) => prev,
  });
}

// ─── Detail ──────────────────────────────────────────────────────────────────

export function useOdaProject(id: string | undefined) {
  return useQuery({
    queryKey: odaProjectKeys.detail(id ?? ''),
    queryFn: () => apiClient.get<OdaProjectDetail>(`${BASE}/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Create ──────────────────────────────────────────────────────────────────

export function useCreateOdaProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateOdaProjectRequest) =>
      apiClient.post<{ id: string }>(BASE, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: odaProjectKeys.all }),
  });
}

// ─── Update ──────────────────────────────────────────────────────────────────

export function useUpdateOdaProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdateOdaProjectRequest) =>
      apiClient.put(`${BASE}/${id}`, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: odaProjectKeys.all }),
  });
}

// ─── Delete ──────────────────────────────────────────────────────────────────

export function useDeleteOdaProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => apiClient.delete(`${BASE}/${id}`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: odaProjectKeys.all }),
  });
}

// ─── Sub-entity mutations ────────────────────────────────────────────────────

export function useAddOdaLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string; provinceId: string; districtId?: string; address?: string }) =>
      apiClient.post(`${BASE}/${projectId}/locations`, body).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: odaProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteOdaLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, locationId }: { projectId: string; locationId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/locations/${locationId}`).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: odaProjectKeys.detail(vars.projectId) }),
  });
}

export function useAddOdaBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/bid-packages`, body).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: odaProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteOdaBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, bidPackageId }: { projectId: string; bidPackageId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/bid-packages/${bidPackageId}`).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: odaProjectKeys.detail(vars.projectId) }),
  });
}

export function useAddOdaDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/documents`, body).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: odaProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteOdaDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, documentId }: { projectId: string; documentId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/documents/${documentId}`).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: odaProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Reuse seed/dynamic catalog hooks from domestic module ───────────────────
export { useSeedCatalog, useDynamicCatalog } from '@/features/domestic-projects/domestic-project-api';
