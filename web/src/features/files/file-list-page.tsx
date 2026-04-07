import { useState } from 'react';
import { Table, Tag, Button, Upload, message, Space, Popconfirm, Flex, Tooltip } from 'antd';
import { UploadOutlined, DownloadOutlined, DeleteOutlined, InboxOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { UploadRequestOption } from 'rc-upload/lib/interface';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useFiles, useUploadFile, useDeleteFile, downloadFile } from './file-api';
import { formatFileSize } from './file-types';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';
import type { FileRecordDto, VirusScanStatus } from './file-types';

const { Dragger } = Upload;

const SCAN_COLOR: Record<VirusScanStatus, string> = {
  Pending: 'processing',
  Clean: 'success',
  Infected: 'error',
  Failed: 'warning',
};

// FileListPage — paginated file table with upload dragger
export function FileListPage() {
  const { t } = useTranslation();
  const [uploadVisible, setUploadVisible] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  // Fetch all data — client-side search needs full dataset
  const { data, isLoading } = useFiles(1, 9999);
  const { mutate: upload, isPending: isUploading } = useUploadFile();
  const deleteMutation = useDeleteFile();

  // Bulk delete — allSettled so partial failures are reported, not silently swallowed
  async function handleBulkDelete() {
    const results = await Promise.allSettled(selectedIds.map(id => deleteMutation.mutateAsync(id)));
    const failed = results.filter(r => r.status === 'rejected').length;
    if (failed > 0) {
      message.warning(t('common.bulkDeletePartialFail', { failed, total: selectedIds.length, defaultValue: `${failed}/${selectedIds.length} mục xóa thất bại` }));
    } else {
      message.success(t('common.bulkDeleteSuccess', { defaultValue: 'Xóa thành công' }));
    }
    setSelectedIds([]);
  }

  function handleUpload(options: UploadRequestOption) {
    const formData = new FormData();
    formData.append('file', options.file as File);
    upload(formData, {
      onSuccess: () => {
        message.success(t('page.files.uploadSuccess'));
        setUploadVisible(false);
        if (options.onSuccess) options.onSuccess({});
      },
      onError: (err) => {
        message.error(t('page.files.uploadError'));
        if (options.onError) options.onError(err as Error);
      },
    });
  }

  // Column dataIndex uses PascalCase to match BE Dapper response (Files module uses raw SQL)
  const columns: ColumnsType<FileRecordDto> = [
    {
      title: t('page.files.col.fileName'),
      dataIndex: 'OriginalFileName',
      key: 'OriginalFileName',
      ellipsis: true,
    },
    {
      title: t('page.files.col.contentType'),
      dataIndex: 'ContentType',
      key: 'ContentType',
      width: 150,
      ellipsis: true,
    },
    {
      title: t('page.files.col.sizeBytes'),
      dataIndex: 'SizeBytes',
      key: 'SizeBytes',
      width: 110,
      render: (v: number) => formatFileSize(v ?? 0),
    },
    {
      title: t('page.files.col.virusScan'),
      dataIndex: 'VirusScanStatus',
      key: 'VirusScanStatus',
      width: 120,
      render: (v: VirusScanStatus) => (
        <Tag color={SCAN_COLOR[v] ?? 'default'}>{t(`page.files.scanStatus.${v ?? 'Pending'}`)}</Tag>
      ),
    },
    {
      title: t('page.files.col.uploadedBy'),
      dataIndex: 'UploadedBy',
      key: 'UploadedBy',
      width: 150,
      ellipsis: true,
    },
    {
      title: t('page.files.col.uploadedAt'),
      dataIndex: 'UploadedAt',
      key: 'UploadedAt',
      width: 130,
      render: (v: string) => v ? dayjs(v).format('DD/MM/YYYY HH:mm') : '—',
    },
    {
      title: '',
      key: 'actions',
      width: 180,
      render: (_: unknown, r: FileRecordDto) => (
        <Space size="small">
          <Tooltip title={t('page.files.download', 'Tải xuống')}>
            <Button size="small" icon={<DownloadOutlined />} onClick={() => downloadFile(r.Id, r.OriginalFileName)} />
          </Tooltip>
          <Popconfirm
            title={t('common.confirm')}
            description={t('common.delete') + '?'}
            onConfirm={() => deleteMutation.mutate(r.Id, {
              onSuccess: () => message.success(t('common.deleteSuccess', { defaultValue: 'Xóa thành công' })),
              onError: () => message.error(t('common.deleteFailed', { defaultValue: 'Xóa thất bại' })),
            })}
            okText={t('common.yes')}
            cancelText={t('common.no')}
          >
            <Button size="small" danger icon={<DeleteOutlined />} loading={deleteMutation.isPending} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <AdminPageHeader
        title={t('page.files.title')}
        stats={{ total: data?.totalCount, label: t('common.items') }}
        actions={
          <Button
            type="primary"
            icon={<UploadOutlined />}
            onClick={() => setUploadVisible(!uploadVisible)}
          >
            {uploadVisible ? t('common.close') : t('page.files.upload')}
          </Button>
        }
      />

      {uploadVisible && (
        <Dragger
          name="file"
          multiple={false}
          customRequest={handleUpload}
          disabled={isUploading}
          style={{ marginBottom: 16 }}
          showUploadList={false}
        >
          <Space direction="vertical" align="center">
            <InboxOutlined style={{ fontSize: 32 }} />
            <p>{t('page.files.dragHint')}</p>
            <p style={{ color: '#888', fontSize: 12 }}>{t('page.files.maxSize')}</p>
          </Space>
        </Dragger>
      )}

      <AdminContentCard noPadding>
        <AdminTableToolbar
          searchPlaceholder={t('common.search', { defaultValue: 'Tìm kiếm...' })}
          searchValue={searchText}
          onSearchChange={setSearchText}
        />
        {selectedIds.length > 0 && (
          <Flex gap={8} style={{ padding: '0 24px 8px' }}>
            <Popconfirm
              title={t('common.bulkDeleteConfirm', { defaultValue: `Xóa ${selectedIds.length} mục đã chọn?` })}
              onConfirm={handleBulkDelete}
              okText={t('common.yes', { defaultValue: 'Có' })}
              cancelText={t('common.no', { defaultValue: 'Không' })}
            >
              <Button danger size="small">
                {t('common.deleteSelected', { defaultValue: `Xóa (${selectedIds.length})` })}
              </Button>
            </Popconfirm>
          </Flex>
        )}
        <Table<FileRecordDto>
          rowKey="Id"
          columns={columns}
          dataSource={(data?.items ?? []).filter(item =>
            !searchText || Object.values(item).some(v =>
              String(v ?? '').toLowerCase().includes(searchText.toLowerCase())
            )
          )}
          loading={isLoading}
          size="small"
          rowSelection={{
            selectedRowKeys: selectedIds,
            onChange: (keys) => setSelectedIds(keys as string[]),
          }}
          pagination={{ pageSize: 20, showSizeChanger: true, showTotal: (n) => t('page.files.showTotal', { count: n }) }}
          locale={{ emptyText: t('page.files.empty') }}
        />
      </AdminContentCard>
    </div>
  );
}
