// form-template-api.ts — React Query hooks for form template + field CRUD operations

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type {
  FormTemplateDto,
  PagedResult,
  AddFieldPayload,
  UpdateFieldPayload,
  FormFieldDto,
  DataSourceOptionDto,
} from './form-types';

export const formQueryKeys = {
  templates: (page: number) => ['forms', 'templates', page] as const,
  template: (id: string) => ['forms', 'template', id] as const,
  submissions: (templateId: string, page: number) => ['forms', 'submissions', templateId, page] as const,
  fieldOptions: (templateId: string, fieldId: string) => ['forms', 'fieldOptions', templateId, fieldId] as const,
};

/** GET /api/v1/forms/templates (paginated) */
export function useFormTemplates(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: formQueryKeys.templates(page),
    queryFn: () =>
      apiClient
        .get<PagedResult<FormTemplateDto>>('/forms/templates', { params: { page, pageSize } })
        .then((r) => r.data),
  });
}

/** GET /api/v1/forms/templates/{id} */
export function useFormTemplate(id: string) {
  return useQuery({
    queryKey: formQueryKeys.template(id),
    queryFn: () =>
      apiClient.get<FormTemplateDto>(`/forms/templates/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

/** GET /api/v1/forms/templates/{id}/fields/{fieldId}/options — dynamic data source options */
export function useFieldOptions(templateId: string, fieldId: string) {
  return useQuery({
    queryKey: formQueryKeys.fieldOptions(templateId, fieldId),
    queryFn: () =>
      apiClient
        .get<DataSourceOptionDto[]>(`/forms/templates/${templateId}/fields/${fieldId}/options`)
        .then((r) => r.data),
    enabled: Boolean(templateId && fieldId),
    staleTime: 5 * 60 * 1000,
  });
}

/** PUT /api/v1/forms/templates/{id}/fields/reorder — bulk reorder all fields atomically */
export function useBulkReorderFields(templateId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (items: Array<{ fieldId: string; newOrder: number }>) =>
      apiClient
        .put(`/forms/templates/${templateId}/fields/reorder`, { items })
        .then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: formQueryKeys.template(templateId) });
    },
  });
}

/** PATCH /api/v1/forms/templates/{id}/fields/{fieldId}/order — reorder a single field (legacy) */
export function useMoveField(templateId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ fieldId, newOrder }: { fieldId: string; newOrder: number }) =>
      apiClient
        .patch(`/forms/templates/${templateId}/fields/${fieldId}/order`, { newOrder })
        .then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: formQueryKeys.template(templateId) });
    },
  });
}

/** POST /api/v1/forms/templates — create a new template */
export function useCreateTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: {
      name: string;
      nameVi: string;
      code: string;
      storageMode?: 'Json' | 'Table';
    }) =>
      apiClient.post<FormTemplateDto>('/forms/templates', {
        ...body,
        storageMode: body.storageMode ?? 'Json',
      }).then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['forms', 'templates'] });
    },
  });
}

/** DELETE /api/v1/forms/templates/{id} */
export function useDeleteTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id }: { id: string }) =>
      apiClient.delete(`/forms/templates/${id}`).then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['forms', 'templates'] });
    },
  });
}

/** POST /api/v1/forms/templates/{id}/publish */
export function usePublishTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/forms/templates/${id}/publish`).then((r) => r.data),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['forms', 'template', id] });
      queryClient.invalidateQueries({ queryKey: ['forms', 'templates'] });
    },
  });
}

/** POST /api/v1/forms/templates/{id}/fields — add a new field to a template */
export function useAddField(templateId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: AddFieldPayload) =>
      apiClient
        .post<FormFieldDto>(`/forms/templates/${templateId}/fields`, body)
        .then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: formQueryKeys.template(templateId) });
    },
  });
}

/** PUT /api/v1/forms/templates/{id}/fields/{fieldId} — update mutable field properties */
export function useUpdateField(templateId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ fieldId, ...body }: UpdateFieldPayload & { fieldId: string }) =>
      apiClient
        .put<FormFieldDto>(`/forms/templates/${templateId}/fields/${fieldId}`, body)
        .then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: formQueryKeys.template(templateId) });
    },
  });
}

/** DELETE /api/v1/forms/templates/{id}/fields/{fieldId} — soft-delete (deactivate) a field */
export function useDeactivateField(templateId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ fieldId }: { fieldId: string }) =>
      apiClient
        .delete(`/forms/templates/${templateId}/fields/${fieldId}`)
        .then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: formQueryKeys.template(templateId) });
    },
  });
}

/** PUT /api/v1/forms/templates/{id} — update template metadata */
export function useUpdateTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      ...body
    }: {
      id: string;
      name?: string;
      nameVi?: string;
      requiresConsent?: boolean;
      consentText?: string | null;
      approvalWorkflowDefinitionId?: string | null;
      /** When true, clears any linked approval workflow */
      clearApprovalWorkflow?: boolean;
    }) => apiClient.put(`/forms/templates/${id}`, body).then((r) => r.data),
    onSuccess: (_data, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['forms', 'template', id] });
      queryClient.invalidateQueries({ queryKey: ['forms', 'templates'] });
    },
  });
}

/** POST /api/v1/forms/templates/{id}/duplicate */
export function useDuplicateTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      name,
      code,
    }: {
      id: string;
      name: string;
      code: string;
    }) =>
      apiClient
        .post<FormTemplateDto>(`/forms/templates/${id}/duplicate`, { name, code })
        .then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['forms', 'templates'] });
    },
  });
}
