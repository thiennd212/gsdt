// Signature API hooks — wraps /api/v1/signatures/* endpoints (P2 module)

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult, PaginationParams } from '@/shared/types/api';

// ─── Types ────────────────────────────────────────────────────────────────────

export type SignatureRequestStatus = 'Pending' | 'InProgress' | 'Completed' | 'Declined' | 'Expired';
export type SignerStatus = 'Waiting' | 'Signed' | 'Declined';

export interface SignerDto {
  id: string;
  userId: string;
  userName: string;
  email: string;
  status: SignerStatus;
  signedAt?: string;
  order: number;
}

export interface SignatureRequestDto {
  id: string;
  title: string;
  documentId: string;
  documentName: string;
  status: SignatureRequestStatus;
  createdAt: string;
  expiresAt?: string;
  signers: SignerDto[];
}

export interface CreateSignatureRequestDto {
  title: string;
  documentId: string;
  signerIds: string[];
  expiresAt?: string;
}

// ─── Query keys ───────────────────────────────────────────────────────────────

export const signatureQueryKeys = {
  all: ['signatures'] as const,
  list: (params: PaginationParams) => ['signatures', 'list', params] as const,
  detail: (id: string) => ['signatures', 'detail', id] as const,
};

// ─── Queries ──────────────────────────────────────────────────────────────────

/** GET /api/v1/signatures — paginated signature request list */
export function useSignatureRequests(params: PaginationParams = {}) {
  return useQuery({
    queryKey: signatureQueryKeys.list(params),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<SignatureRequestDto>>('/signatures', { params })
        .then((r) => r.data),
    placeholderData: (prev) => prev,
  });
}

/** GET /api/v1/signatures/{id} */
export function useSignatureRequest(id: string) {
  return useQuery({
    queryKey: signatureQueryKeys.detail(id),
    queryFn: () =>
      apiClient.get<SignatureRequestDto>(`/signatures/${id}`).then((r) => r.data),
    enabled: Boolean(id),
  });
}

// ─── Mutations ────────────────────────────────────────────────────────────────

/** POST /api/v1/signatures */
export function useCreateSignatureRequest() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateSignatureRequestDto) =>
      apiClient.post<SignatureRequestDto>('/signatures', body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: signatureQueryKeys.all }),
  });
}

/** POST /api/v1/signatures/{id}/cancel */
export function useCancelSignatureRequest() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/signatures/${id}/cancel`).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: signatureQueryKeys.all }),
  });
}
