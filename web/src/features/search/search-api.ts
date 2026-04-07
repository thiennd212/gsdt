// Unified search API hooks — wraps /api/v1/search/* endpoints

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { FacetGroup } from '@/shared/components/facet-sidebar';

// ─── Types ────────────────────────────────────────────────────────────────────

export type SearchEntityType = 'case' | 'file' | 'form' | 'user' | 'notification';

export interface SearchResultItem {
  id: string;
  entityType: SearchEntityType;
  title: string;
  summary?: string;
  status?: string;
  updatedAt?: string;
  url?: string;
}

export interface UnifiedSearchResponse {
  items: SearchResultItem[];
  totalCount: number;
  facets: FacetGroup[];
}

export interface UnifiedSearchParams {
  q: string;
  entityTypes?: string[];
  facets?: Record<string, string[]>;
  pageNumber?: number;
  pageSize?: number;
}

// ─── Query keys ───────────────────────────────────────────────────────────────

export const searchQueryKeys = {
  unified: (params: UnifiedSearchParams) => ['search', 'unified', params] as const,
};

// ─── Queries ──────────────────────────────────────────────────────────────────

/** GET /api/v1/search — unified multi-entity search with facets */
export function useUnifiedSearch(params: UnifiedSearchParams) {
  // Serialize facets map to query-friendly format
  const queryParams = {
    q: params.q,
    entityTypes: params.entityTypes,
    pageNumber: params.pageNumber ?? 1,
    pageSize: params.pageSize ?? 20,
    ...Object.fromEntries(
      Object.entries(params.facets ?? {}).map(([k, v]) => [`facet_${k}`, v.join(',')]),
    ),
  };

  return useQuery({
    queryKey: searchQueryKeys.unified(params),
    queryFn: () =>
      apiClient
        .get<UnifiedSearchResponse>('/search', { params: queryParams })
        .then((r) => r.data),
    enabled: params.q.trim().length >= 2,
    placeholderData: (prev) => prev,
  });
}
