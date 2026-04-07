// form-public-api.ts — React Query hooks for public forms + workflow definitions

import axios from 'axios';
import { useQuery, useMutation } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { FormTemplateDto } from './form-types';

// Plain axios instance for anonymous/public endpoints — does NOT attach Bearer tokens or
// trigger userManager.getUser(). This prevents auth errors for unauthenticated form visitors.
const publicClient = axios.create({
  baseURL: (import.meta.env.VITE_API_BASE_URL as string) || '/api/v1',
  headers: { 'Content-Type': 'application/json' },
});

/** GET /api/v1/public/forms/{code} — anonymous form schema fetch */
export function usePublicForm(code: string) {
  return useQuery({
    queryKey: ['public-form', code] as const,
    queryFn: () =>
      publicClient.get<FormTemplateDto>(`/public/forms/${code}`).then((r) => r.data),
    enabled: Boolean(code),
  });
}

/** POST /api/v1/public/forms/{code}/submit — anonymous submission */
export function useSubmitPublicForm(code: string) {
  return useMutation({
    mutationFn: ({ data, consentGiven }: { data: Record<string, unknown>; consentGiven?: boolean }) =>
      publicClient
        .post<{ id: string }>(`/public/forms/${code}/submit`, {
          data,
          ...(consentGiven !== undefined ? { consentGiven } : {}),
        })
        .then((r) => r.data),
  });
}

/** GET /api/v1/workflow/definitions?isActive=true — list active workflow definitions */
export function useWorkflowDefinitions() {
  return useQuery({
    queryKey: ['workflow', 'definitions'] as const,
    queryFn: () =>
      apiClient
        .get('/workflow/definitions', { params: { isActive: true, page: 1, pageSize: 100 } })
        .then((r) => r.data),
    staleTime: 5 * 60 * 1000,
  });
}
