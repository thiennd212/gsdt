// Attachment preview for File-type fields in submission detail.
// Fetches file metadata from GET /api/v1/files/{fileId} to determine MIME type.
// Images: <img> thumbnail (max 200px)
// PDFs: PDF icon + filename link
// Others: generic file icon + download link
import { Spin, Tooltip } from 'antd';
import { FilePdfOutlined, FileOutlined } from '@ant-design/icons';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { FileRecordDto } from '@/features/files/file-types';

interface Props {
  fileId: string;
}

function useFileMetadata(fileId: string) {
  return useQuery({
    queryKey: ['files', 'file', fileId] as const,
    queryFn: () =>
      apiClient.get<FileRecordDto>(`/files/${fileId}`).then((r) => r.data),
    enabled: Boolean(fileId),
    staleTime: 5 * 60 * 1000, // 5 min — file metadata rarely changes
  });
}

export function AttachmentPreview({ fileId }: Props) {
  const { data: file, isLoading } = useFileMetadata(fileId);

  if (isLoading) return <Spin size="small" />;
  if (!file) return <span style={{ color: '#999' }}>File not found</span>;

  const contentType = file.ContentType;
  const fileName = file.OriginalFileName;
  const href = `/api/v1/files/${fileId}/download`;

  // Image preview
  if (contentType.startsWith('image/')) {
    return (
      <Tooltip title={fileName}>
        <a href={href} target="_blank" rel="noopener noreferrer">
          <img
            src={href}
            alt={fileName}
            style={{ maxWidth: 200, maxHeight: 200, objectFit: 'contain', border: '1px solid #eee', borderRadius: 4 }}
          />
        </a>
      </Tooltip>
    );
  }

  // PDF preview
  if (contentType === 'application/pdf') {
    return (
      <Tooltip title={fileName}>
        <a href={href} target="_blank" rel="noopener noreferrer" style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
          <FilePdfOutlined style={{ fontSize: 24, color: '#ff4d4f' }} />
          <span>{fileName}</span>
        </a>
      </Tooltip>
    );
  }

  // Generic file
  return (
    <Tooltip title={fileName}>
      <a href={href} target="_blank" rel="noopener noreferrer" style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
        <FileOutlined style={{ fontSize: 20, color: '#1677ff' }} />
        <span>{fileName}</span>
      </a>
    </Tooltip>
  );
}

/** Render a field value that might be a file ID or array of file IDs */
export function FieldValueWithAttachment({ fieldKey, value }: { fieldKey: string; value: unknown }) {
  void fieldKey; // reserved for future label lookup

  if (!value) return <span style={{ color: '#999' }}>—</span>;

  const strVal = String(value);

  // Detect GUID-shaped file IDs (36 chars with hyphens)
  const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
  if (guidRegex.test(strVal)) {
    return <AttachmentPreview fileId={strVal} />;
  }

  // Plain value
  return <span>{strVal}</span>;
}
