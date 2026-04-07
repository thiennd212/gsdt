// Files API hooks — React Query wrappers for files endpoints

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { FileRecordDto, PagedResult } from './file-types';

export const fileQueryKeys = {
  list: (page: number) => ['files', 'list', page] as const,
  file: (id: string) => ['files', 'file', id] as const,
};

/** GET /api/v1/files (paginated) */
export function useFiles(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: fileQueryKeys.list(page),
    queryFn: () =>
      apiClient
        .get<PagedResult<FileRecordDto>>('/files', { params: { page, pageSize } })
        .then((r) => r.data),
  });
}

/** POST /api/v1/files — multipart upload */
export function useUploadFile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (formData: FormData) =>
      apiClient
        .post<FileRecordDto>('/files', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        })
        .then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['files', 'list'] });
    },
  });
}

/** DELETE /api/v1/files/{id} — soft-delete */
export function useDeleteFile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.delete(`/files/${id}`).then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['files', 'list'] });
    },
  });
}

/** Trigger browser download for a file */
export function downloadFile(id: string, fileName: string) {
  const url = `/api/v1/files/${id}/download`;
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
}

/** GET /api/v1/files/{id}/download — returns download URL for preview */
export function getFileDownloadUrl(id: string): string {
  return `/api/v1/files/${id}/download`;
}
