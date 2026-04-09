// Permissions API hooks — wraps /api/v1/admin/permissions and role-permission assignment endpoints

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface PermissionDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  moduleCode: string;
  resourceCode: string;
  actionCode: string;
}

export interface ModulePermissionsDto {
  moduleCode: string;
  permissions: PermissionDto[];
}

export interface AssignPermissionsRequest {
  permissionIds: string[];
}

// ─── Query keys ───────────────────────────────────────────────────────────────

export const permissionsQueryKeys = {
  all: ['permissions'] as const,
  byModule: ['permissions', 'by-module'] as const,
  forRole: (roleId: string) => ['permissions', 'role', roleId] as const,
};

// ─── Queries ──────────────────────────────────────────────────────────────────

/** GET /api/v1/admin/permissions/by-module — all permissions grouped by module */
export function usePermissionsByModule() {
  return useQuery({
    queryKey: permissionsQueryKeys.byModule,
    queryFn: () =>
      apiClient
        .get<ModulePermissionsDto[]>('/admin/permissions/by-module')
        .then((r) => r.data),
    staleTime: 10 * 60 * 1000, // permissions catalogue is stable
  });
}

/** GET /api/v1/admin/roles/{roleId}/permissions — permissions currently assigned to a role */
export function useRolePermissions(roleId: string | null) {
  return useQuery({
    queryKey: permissionsQueryKeys.forRole(roleId ?? ''),
    queryFn: () =>
      apiClient
        .get<PermissionDto[]>(`/admin/roles/${roleId}/permissions`)
        .then((r) => r.data),
    enabled: Boolean(roleId),
  });
}

// ─── Mutations ────────────────────────────────────────────────────────────────

/** POST /api/v1/admin/roles/{roleId}/permissions — assign permissions to a role */
export function useAssignPermissions(roleId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: AssignPermissionsRequest) =>
      apiClient
        .post<PermissionDto[]>(`/admin/roles/${roleId}/permissions`, body)
        .then((r) => r.data),
    onSuccess: () =>
      qc.invalidateQueries({ queryKey: permissionsQueryKeys.forRole(roleId) }),
  });
}

/** DELETE /api/v1/admin/roles/{roleId}/permissions — remove permissions from a role */
export function useRemovePermissions(roleId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: AssignPermissionsRequest) =>
      apiClient
        .delete(`/admin/roles/${roleId}/permissions`, { data: body })
        .then((r) => r.data),
    onSuccess: () =>
      qc.invalidateQueries({ queryKey: permissionsQueryKeys.forRole(roleId) }),
  });
}
