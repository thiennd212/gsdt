import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult } from '@/shared/types/api';
import type {
  DomesticProjectListItem,
  DomesticProjectListParams,
  DomesticProjectDetail,
  CreateDomesticProjectRequest,
  UpdateDomesticProjectRequest,
  SeedCatalogItem,
} from './domestic-project-types';

const BASE = '/domestic-projects';

// ─── Query keys ──────────────────────────────────────────────────────────────

export const domesticProjectKeys = {
  all: ['domestic-projects'] as const,
  list: (params: DomesticProjectListParams) => ['domestic-projects', 'list', params] as const,
  detail: (id: string) => ['domestic-projects', 'detail', id] as const,
  seed: (type: string) => ['catalogs', type] as const,
};

// ─── List query ──────────────────────────────────────────────────────────────

export function useDomesticProjects(params: DomesticProjectListParams) {
  return useQuery({
    queryKey: domesticProjectKeys.list(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<DomesticProjectListItem>>(BASE, { params })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });
}

// ─── Detail query ────────────────────────────────────────────────────────────

export function useDomesticProject(id: string | undefined) {
  return useQuery({
    queryKey: domesticProjectKeys.detail(id ?? ''),
    queryFn: () =>
      apiClient.get<DomesticProjectDetail>(`${BASE}/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Create ──────────────────────────────────────────────────────────────────

export function useCreateDomesticProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateDomesticProjectRequest) =>
      apiClient.post<{ id: string }>(BASE, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: domesticProjectKeys.all }),
  });
}

// ─── Update ──────────────────────────────────────────────────────────────────

export function useUpdateDomesticProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...body }: UpdateDomesticProjectRequest) =>
      apiClient.put(`${BASE}/${id}`, body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: domesticProjectKeys.all }),
  });
}

// ─── Delete ──────────────────────────────────────────────────────────────────

export function useDeleteDomesticProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`${BASE}/${id}`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: domesticProjectKeys.all }),
  });
}

// ─── Sub-entity mutations ────────────────────────────────────────────────────

export function useAddLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string; provinceId: string; districtId?: string; wardId?: string; address?: string }) =>
      apiClient.post(`${BASE}/${projectId}/locations`, body).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: domesticProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteLocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, locationId }: { projectId: string; locationId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/locations/${locationId}`).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: domesticProjectKeys.detail(vars.projectId) }),
  });
}

export function useAddDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/decisions`, body).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: domesticProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDecision() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, decisionId }: { projectId: string; decisionId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/decisions/${decisionId}`).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: domesticProjectKeys.detail(vars.projectId) }),
  });
}

export function useAddBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/bid-packages`, body).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: domesticProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteBidPackage() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, bidPackageId }: { projectId: string; bidPackageId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/bid-packages/${bidPackageId}`).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: domesticProjectKeys.detail(vars.projectId) }),
  });
}

export function useAddDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, ...body }: { projectId: string } & Record<string, unknown>) =>
      apiClient.post(`${BASE}/${projectId}/documents`, body).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: domesticProjectKeys.detail(vars.projectId) }),
  });
}

export function useDeleteDocument() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, documentId }: { projectId: string; documentId: string }) =>
      apiClient.delete(`${BASE}/${projectId}/documents/${documentId}`).then((r) => r.data),
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: domesticProjectKeys.detail(vars.projectId) }),
  });
}

// ─── Seed catalog queries (shared by dropdowns) ─────────────────────────────

export function useSeedCatalog(type: string) {
  return useQuery({
    queryKey: domesticProjectKeys.seed(type),
    queryFn: () =>
      apiClient
        .get<SeedCatalogItem[]>(`/masterdata/catalogs/${type}`)
        .then((r) => r.data),
    staleTime: 10 * 60 * 1000, // seed data rarely changes
  });
}

// ─── Dynamic catalog queries (tenant-scoped) ────────────────────────────────

export function useDynamicCatalog(type: string) {
  return useQuery({
    queryKey: ['catalogs', type],
    queryFn: () =>
      apiClient
        .get<{ id: string; code: string; name: string; isActive: boolean }[]>(
          `/masterdata/catalogs/${type}`,
        )
        .then((r) => r.data),
    staleTime: 5 * 60 * 1000,
  });
}
