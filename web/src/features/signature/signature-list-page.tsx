import { useState } from 'react';
import { Table, Button, Space, Typography, Tag, Modal, Form, Input, DatePicker, Popconfirm, message, Drawer, Flex } from 'antd';
import { PlusOutlined, EyeOutlined, StopOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';
import {
  useSignatureRequests,
  useSignatureRequest,
  useCreateSignatureRequest,
  useCancelSignatureRequest,
  type SignatureRequestDto,
  type SignatureRequestStatus,
  type SignerDto,
  type SignerStatus,
  type CreateSignatureRequestDto,
} from './signature-api';

const { Title, Text } = Typography;

const REQUEST_STATUS_COLOR: Record<SignatureRequestStatus, string> = {
  Pending: 'blue',
  InProgress: 'processing',
  Completed: 'green',
  Declined: 'red',
  Expired: 'default',
};

const SIGNER_STATUS_COLOR: Record<SignerStatus, string> = {
  Waiting: 'orange',
  Signed: 'green',
  Declined: 'red',
};

// ─── Detail drawer — signer progress tracker ─────────────────────────────────

function SignatureDetailDrawer({ requestId, open, onClose }: { requestId: string; open: boolean; onClose: () => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useSignatureRequest(requestId);

  const signerColumns: ColumnsType<SignerDto> = [
    { title: t('signatures.col.order'), dataIndex: 'order', key: 'order', width: 60 },
    { title: t('signatures.col.signerName'), dataIndex: 'userName', key: 'userName' },
    { title: t('signatures.col.email'), dataIndex: 'email', key: 'email', ellipsis: true },
    {
      title: t('signatures.col.signerStatus'),
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (v: SignerStatus) => <Tag color={SIGNER_STATUS_COLOR[v]}>{v}</Tag>,
    },
    {
      title: t('signatures.col.signedAt'),
      dataIndex: 'signedAt',
      key: 'signedAt',
      width: 140,
      render: (v?: string) => (v ? dayjs(v).format('DD/MM/YYYY HH:mm') : '—'),
    },
  ];

  return (
    <Drawer
      title={data?.title ?? t('signatures.detailTitle')}
      open={open}
      onClose={onClose}
      width={560}
    >
      {data && (
        <Space direction="vertical" style={{ width: '100%' }} size={16}>
          <Space wrap>
            <Text strong>{t('signatures.col.status')}:</Text>
            <Tag color={REQUEST_STATUS_COLOR[data.status]}>{data.status}</Tag>
            <Text strong>{t('signatures.col.document')}:</Text>
            <Text>{data.documentName}</Text>
          </Space>
          {data.expiresAt && (
            <Text type="secondary">{t('signatures.expiresAt')}: {dayjs(data.expiresAt).format('DD/MM/YYYY')}</Text>
          )}
          <Title level={5} style={{ marginBottom: 0 }}>{t('signatures.signersSection')}</Title>
          <Table<SignerDto>
            rowKey="id"
            columns={signerColumns}
            dataSource={data.signers.sort((a, b) => a.order - b.order)}
            loading={isLoading}
            size="small"
            pagination={false}
          />
        </Space>
      )}
    </Drawer>
  );
}

// ─── Create form modal ────────────────────────────────────────────────────────

function CreateRequestModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { t } = useTranslation();
  const [form] = Form.useForm<CreateSignatureRequestDto & { signerIdsText: string }>();
  const createMutation = useCreateSignatureRequest();

  async function handleSave() {
    const values = await form.validateFields();
    // Accept comma-separated signer IDs in the text field
    const signerIds = values.signerIdsText.split(',').map((s) => s.trim()).filter(Boolean);
    try {
      await createMutation.mutateAsync({
        title: values.title,
        documentId: values.documentId,
        signerIds,
        expiresAt: values.expiresAt,
      });
      message.success(t('signatures.createSuccess'));
      form.resetFields();
      onClose();
    } catch {
      message.error(t('common.error'));
    }
  }

  return (
    <Modal
      title={t('signatures.createTitle')}
      open={open}
      onOk={handleSave}
      onCancel={() => { form.resetFields(); onClose(); }}
      confirmLoading={createMutation.isPending}
      okText={t('common.save')}
      cancelText={t('common.cancel')}
      destroyOnHidden
    >
      <Form form={form} layout="vertical" style={{ marginTop: 8 }}>
        <Form.Item name="title" label={t('signatures.col.title')} rules={[{ required: true }]}>
          <Input />
        </Form.Item>
        <Form.Item name="documentId" label={t('signatures.col.documentId')} rules={[{ required: true }]}>
          <Input placeholder={t('signatures.documentIdPlaceholder')} />
        </Form.Item>
        <Form.Item name="signerIdsText" label={t('signatures.signersLabel')} rules={[{ required: true }]}
          extra={t('signatures.signersHint')}>
          <Input.TextArea rows={3} placeholder="user-id-1, user-id-2" />
        </Form.Item>
        <Form.Item name="expiresAt" label={t('signatures.expiresAtLabel')}>
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>
      </Form>
    </Modal>
  );
}

// ─── List page ────────────────────────────────────────────────────────────────

// SignatureListPage — list signature requests with status badges and signer detail drawer
export function SignatureListPage() {
  const { t } = useTranslation();
  const [createOpen, setCreateOpen] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [detailId, setDetailId] = useState<string | null>(null);

  // Fetch all data — client-side search needs full dataset
  const { data, isFetching } = useSignatureRequests({ pageNumber: 1, pageSize: 9999 });
  const cancelMutation = useCancelSignatureRequest();

  const items = data?.items ?? [];

  async function handleBulkCancel() {
    const results = await Promise.allSettled(selectedIds.map((id) => cancelMutation.mutateAsync(id)));
    const failed = results.filter(r => r.status === 'rejected').length;
    if (failed > 0) {
      message.warning(t('common.bulkDeletePartialFail', { failed, total: selectedIds.length, defaultValue: `${failed}/${selectedIds.length} mục thất bại` }));
    } else {
      message.success(t('signatures.cancelSuccess', 'Hủy yêu cầu ký thành công'));
    }
    setSelectedIds([]);
  }

  const columns: ColumnsType<SignatureRequestDto> = [
    { title: t('signatures.col.title'), dataIndex: 'title', key: 'title', ellipsis: true },
    { title: t('signatures.col.document'), dataIndex: 'documentName', key: 'documentName', ellipsis: true, width: 180 },
    {
      title: t('signatures.col.status'),
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (v: SignatureRequestStatus) => <Tag color={REQUEST_STATUS_COLOR[v]}>{v}</Tag>,
    },
    {
      title: t('signatures.col.signerCount'),
      key: 'signerCount',
      width: 100,
      render: (_, r) => {
        const signed = r.signers.filter((s) => s.status === 'Signed').length;
        return `${signed}/${r.signers.length}`;
      },
    },
    {
      title: t('signatures.col.createdAt'),
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 130,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
    {
      title: '',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Button size="small" icon={<EyeOutlined />} onClick={() => setDetailId(record.id)} />
          {(record.status === 'Pending' || record.status === 'InProgress') && (
            <Popconfirm
              title={t('signatures.cancelConfirm')}
              onConfirm={() => cancelMutation.mutate(record.id, {
                onSuccess: () => message.success(t('signatures.cancelSuccess', 'Hủy yêu cầu ký thành công')),
                onError: () => message.error(t('signatures.cancelError', 'Hủy yêu cầu ký thất bại')),
              })}
              okText={t('common.confirm')}
              cancelText={t('common.cancel')}
            >
              <Button size="small" icon={<StopOutlined />} danger loading={cancelMutation.isPending} />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <AdminPageHeader
        title={t('signatures.title')}
        stats={{ total: items.length, label: t('common.items') }}
        actions={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            {t('common.add')}
          </Button>
        }
      />
      <AdminContentCard noPadding>
        <AdminTableToolbar
          searchPlaceholder={t('common.search', { defaultValue: 'Tìm kiếm...' })}
          searchValue={searchText}
          onSearchChange={setSearchText}
        />
        {selectedIds.length > 0 && (
          <Flex gap={8} style={{ padding: '0 24px 8px' }}>
            <Popconfirm
              title={t('common.bulkCancelConfirm', { defaultValue: `Hủy ${selectedIds.length} yêu cầu đã chọn?` })}
              onConfirm={handleBulkCancel}
            >
              <Button danger size="small">
                {t('common.cancelSelected', { defaultValue: `Hủy (${selectedIds.length})` })}
              </Button>
            </Popconfirm>
          </Flex>
        )}
        <Table<SignatureRequestDto>
          rowKey="id"
          rowSelection={{
            selectedRowKeys: selectedIds,
            onChange: (keys) => setSelectedIds(keys as string[]),
          }}
          columns={columns}
          dataSource={items.filter(item =>
            !searchText || Object.values(item).some(v =>
              String(v ?? '').toLowerCase().includes(searchText.toLowerCase())
            )
          )}
          loading={isFetching}
          size="small"
          scroll={{ x: 800 }}
          pagination={{ pageSize: 20, showSizeChanger: true, showTotal: (t: number) => `Tổng ${t}` }}
        />
      </AdminContentCard>

      <CreateRequestModal open={createOpen} onClose={() => setCreateOpen(false)} />

      {detailId && (
        <SignatureDetailDrawer
          requestId={detailId}
          open={Boolean(detailId)}
          onClose={() => setDetailId(null)}
        />
      )}
    </div>
  );
}
