// Roles API hooks — wraps /api/v1/admin/roles (Identity module)

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface RoleDefinitionDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  roleType: string;
  isActive: boolean;
  permissionCount: number;
}

export interface RoleDetailDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  roleType: string;
  isActive: boolean;
  permissions: PermissionRef[];
}

export interface PermissionRef {
  id: string;
  code: string;
  name: string;
}

export interface CreateRoleRequest {
  code: string;
  name: string;
  description?: string;
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
}

// ─── Query keys ───────────────────────────────────────────────────────────────

export const rolesQueryKeys = {
  all: ['roles'] as const,
  list: ['roles', 'list'] as const,
  detail: (id: string) => ['roles', 'detail', id] as const,
};

// ─── Queries ──────────────────────────────────────────────────────────────────

/** GET /api/v1/admin/roles — role catalogue (Admin-only endpoint) */
export function useRoles() {
  return useQuery({
    queryKey: rolesQueryKeys.list,
    queryFn: () =>
      apiClient.get<RoleDefinitionDto[]>('/admin/roles').then((r) => r.data),
    staleTime: 5 * 60 * 1000,
  });
}

/** GET /api/v1/admin/roles/{id} — single role with permissions */
export function useRoleById(id: string | null) {
  return useQuery({
    queryKey: rolesQueryKeys.detail(id ?? ''),
    queryFn: () =>
      apiClient.get<RoleDetailDto>(`/admin/roles/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Mutations ────────────────────────────────────────────────────────────────

/** POST /api/v1/admin/roles */
export function useCreateRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRoleRequest) =>
      apiClient.post<RoleDetailDto>('/admin/roles', data).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: rolesQueryKeys.all }),
  });
}

/** PUT /api/v1/admin/roles/{id} */
export function useUpdateRole(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateRoleRequest) =>
      apiClient.put<RoleDetailDto>(`/admin/roles/${id}`, data).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: rolesQueryKeys.all }),
  });
}

/** DELETE /api/v1/admin/roles/{id} */
export function useDeleteRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`/admin/roles/${id}`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: rolesQueryKeys.all }),
  });
}
