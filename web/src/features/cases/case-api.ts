import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult } from '@/shared/types/api';
import type {
  CaseDto,
  CaseListParams,
  CreateCaseRequest,
  AssignCaseRequest,
  ResolveRequest,
  AddCommentRequest,
  CaseComment,
} from './case-types';

// ─── Query keys ──────────────────────────────────────────────────────────────

export const caseQueryKeys = {
  all: ['cases'] as const,
  list: (params: CaseListParams) => ['cases', 'list', params] as const,
  detail: (id: string) => ['cases', 'detail', id] as const,
  comments: (id: string) => ['cases', 'comments', id] as const,
};

// ─── Queries ─────────────────────────────────────────────────────────────────

/** GET /api/v1/cases — paginated + filterable */
export function useCases(params: CaseListParams) {
  return useQuery({
    queryKey: caseQueryKeys.list(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<CaseDto>>('/cases', { params })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });
}

/** GET /api/v1/cases/{id} */
export function useCase(id: string) {
  return useQuery({
    queryKey: caseQueryKeys.detail(id),
    queryFn: () =>
      apiClient.get<CaseDto>(`/cases/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Mutations ───────────────────────────────────────────────────────────────

/** POST /api/v1/cases */
export function useCreateCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateCaseRequest) =>
      apiClient.post<CaseDto>('/cases', body).then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.all });
    },
  });
}

/** POST /api/v1/cases/{id}/submit */
export function useSubmitCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/cases/${id}/submit`).then((r) => r.data),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.all });
    },
  });
}

/** POST /api/v1/cases/{id}/assign */
export function useAssignCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: AssignCaseRequest }) =>
      apiClient.post(`/cases/${id}/assign`, body).then((r) => r.data),
    onSuccess: (_data, { id }) => {
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.all });
    },
  });
}

/** POST /api/v1/cases/{id}/approve */
export function useApproveCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: ResolveRequest }) =>
      apiClient.post(`/cases/${id}/approve`, body).then((r) => r.data),
    onSuccess: (_data, { id }) => {
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.all });
    },
  });
}

/** POST /api/v1/cases/{id}/reject */
export function useRejectCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: ResolveRequest }) =>
      apiClient.post(`/cases/${id}/reject`, body).then((r) => r.data),
    onSuccess: (_data, { id }) => {
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.all });
    },
  });
}

/** POST /api/v1/cases/{id}/close */
export function useCloseCase() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/cases/${id}/close`).then((r) => r.data),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.all });
    },
  });
}

/** POST /api/v1/cases/{id}/comments */
export function useAddComment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: AddCommentRequest }) =>
      apiClient.post<CaseComment>(`/cases/${id}/comments`, body).then((r) => r.data),
    onSuccess: (_data, { id }) => {
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.comments(id) });
      queryClient.invalidateQueries({ queryKey: caseQueryKeys.detail(id) });
    },
  });
}
