import { useState } from 'react';
import { Table, Button, Space, Popconfirm, Tag, Modal, Form, Input, Switch, DatePicker, message, Flex } from 'antd';
import { PlusOutlined, StopOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { useTranslation } from 'react-i18next';
import {
  useDelegations,
  useCreateDelegation,
  useRevokeDelegation,
  type DelegationDto,
  type CreateDelegationDto,
} from './delegation-api';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';

const { RangePicker } = DatePicker;

// Delegation management page — list active delegations, create new, revoke existing
export function DelegationListPage() {
  const { t } = useTranslation();
  const [activeOnly, setActiveOnly] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [form] = Form.useForm();

  const { data, isLoading } = useDelegations(activeOnly);
  const createDelegation = useCreateDelegation();
  const revokeDelegation = useRevokeDelegation();

  const delegations = data ?? [];
  const total = delegations.length;

  // Bulk revoke — allSettled so partial failures are reported, not silently swallowed
  async function handleBulkRevoke() {
    const results = await Promise.allSettled(selectedIds.map(id => revokeDelegation.mutateAsync(id)));
    const failed = results.filter(r => r.status === 'rejected').length;
    if (failed > 0) {
      message.warning(t('common.bulkDeletePartialFail', { failed, total: selectedIds.length, defaultValue: `${failed}/${selectedIds.length} mục thất bại` }));
    } else {
      message.success(t('common.operationSuccess', { defaultValue: 'Thao tác thành công' }));
    }
    setSelectedIds([]);
  }

  const columns: ColumnsType<DelegationDto> = [
    { title: t('delegations.col.delegator'), dataIndex: 'delegatorName', key: 'delegatorName', width: 160, ellipsis: true, render: (v, r) => v ?? r.delegatorId },
    { title: t('delegations.col.delegate'), dataIndex: 'delegateName', key: 'delegateName', width: 160, ellipsis: true, render: (v, r) => v ?? r.delegateId },
    { title: t('delegations.col.validFrom'), dataIndex: 'validFrom', key: 'validFrom', width: 130, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: t('delegations.col.validTo'), dataIndex: 'validTo', key: 'validTo', width: 130, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: t('delegations.col.reason'), dataIndex: 'reason', key: 'reason', ellipsis: true, render: (v) => v ?? '—' },
    {
      title: t('delegations.col.status'),
      dataIndex: 'isActive',
      key: 'isActive',
      width: 110,
      render: (v: boolean) => <Tag color={v ? 'green' : 'default'}>{v ? t('delegations.active') : t('delegations.inactive')}</Tag>,
    },
    {
      title: t('common.delete'),
      key: 'actions',
      width: 90,
      render: (_, record) => (
        <Popconfirm
          title={t('delegations.revokeConfirm')}
          onConfirm={() => revokeDelegation.mutate(record.id, {
            onSuccess: () => message.success(t('common.operationSuccess', { defaultValue: 'Thao tác thành công' })),
            onError: () => message.error(t('common.operationFailed', { defaultValue: 'Thao tác thất bại' })),
          })}
          okText={t('common.confirm')}
          cancelText={t('common.cancel')}
        >
          <Button danger icon={<StopOutlined />} size="small" loading={revokeDelegation.isPending} />
        </Popconfirm>
      ),
    },
  ];

  const handleCreate = async () => {
    const values = await form.validateFields();
    const [validFrom, validTo] = values.dateRange;
    try {
      await createDelegation.mutateAsync({
        delegatorId: values.delegatorId,
        delegateId: values.delegateId,
        validFrom: validFrom.toISOString(),
        validTo: validTo.toISOString(),
        reason: values.reason,
      } as CreateDelegationDto);
      message.success(t('common.operationSuccess', { defaultValue: 'Thao tác thành công' }));
      form.resetFields();
      setModalOpen(false);
    } catch {
      message.error(t('common.operationFailed', { defaultValue: 'Thao tác thất bại' }));
    }
  };

  return (
    <div>
      <AdminPageHeader
        title={t('delegations.title')}
        stats={{ total, label: t('common.items') }}
        actions={
          <Space>
            <Switch
              checked={activeOnly}
              onChange={setActiveOnly}
              checkedChildren={t('delegations.activeOnly')}
              unCheckedChildren={t('delegations.all')}
            />
            <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>
              {t('common.add')}
            </Button>
          </Space>
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
              title={t('common.bulkDeleteConfirm', { defaultValue: `Thu hồi ${selectedIds.length} ủy quyền đã chọn?` })}
              onConfirm={handleBulkRevoke}
              okText={t('common.yes', { defaultValue: 'Có' })}
              cancelText={t('common.no', { defaultValue: 'Không' })}
            >
              <Button danger size="small">
                {t('common.revokeSelected', { defaultValue: `Thu hồi (${selectedIds.length})` })}
              </Button>
            </Popconfirm>
          </Flex>
        )}
        <Table<DelegationDto>
          rowKey="id"
          columns={columns}
          dataSource={delegations.filter(item =>
            !searchText || Object.values(item).some(v =>
              String(v ?? '').toLowerCase().includes(searchText.toLowerCase())
            )
          )}
          loading={isLoading}
          size="small"
          pagination={{ pageSize: 20, showSizeChanger: true, showTotal: (t: number) => `Tổng ${t}` }}
          rowSelection={{
            selectedRowKeys: selectedIds,
            onChange: (keys) => setSelectedIds(keys as string[]),
          }}
        />
      </AdminContentCard>

      <Modal
        title={t('delegations.createTitle')}
        open={modalOpen}
        onOk={handleCreate}
        onCancel={() => { setModalOpen(false); form.resetFields(); }}
        confirmLoading={createDelegation.isPending}
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" style={{ marginTop: 8 }}>
          <Form.Item name="delegatorId" label={t('delegations.col.delegator')} rules={[{ required: true }]}>
            <Input placeholder={t('delegations.userIdPlaceholder')} />
          </Form.Item>
          <Form.Item name="delegateId" label={t('delegations.col.delegate')} rules={[{ required: true }]}>
            <Input placeholder={t('delegations.userIdPlaceholder')} />
          </Form.Item>
          <Form.Item name="dateRange" label={t('delegations.validRange')} rules={[{ required: true }]}>
            <RangePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="reason" label={t('delegations.col.reason')}>
            <Input.TextArea rows={2} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
