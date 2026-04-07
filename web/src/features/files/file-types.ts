// Files feature types — mirrors backend DTOs

export type VirusScanStatus = 'Pending' | 'Clean' | 'Infected' | 'Failed';

// Field names match BE Dapper response (PascalCase — Files module uses raw SQL, not EF serializer)
export interface FileRecordDto {
  Id: string;
  OriginalFileName: string;
  ContentType: string;
  SizeBytes: number;
  UploadedBy: string;
  UploadedAt: string;
  VirusScanStatus: VirusScanStatus;
  IsDigitallySigned?: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/** Human-readable file size (e.g. "1.2 MB") */
export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
