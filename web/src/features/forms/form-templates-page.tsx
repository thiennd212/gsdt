import { useState } from 'react';
import { Table, Tag, Button, Space, Modal, Form, Input, Popconfirm, message, Flex, Tooltip } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useFormTemplates, useCreateTemplate, useDeleteTemplate } from './form-api';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';
import type { FormTemplateDto, FormStatus } from './form-types';

const STATUS_COLOR: Record<FormStatus, string> = {
  Draft: 'default',
  Materializing: 'blue',
  Active: 'green',
  Inactive: 'orange',
};

// FormTemplatesPage — list all form templates; supports create and delete (Draft only)
export function FormTemplatesPage() {
  const { t } = useTranslation();
  const [modalOpen, setModalOpen] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [form] = Form.useForm<{ name: string; nameVi: string; code: string }>();
  const navigate = useNavigate();

  // Fetch all data — client-side search needs full dataset
  const { data, isLoading } = useFormTemplates(1, 9999);
  const createTemplate = useCreateTemplate();
  const deleteTemplate = useDeleteTemplate();

  // Bulk delete — allSettled so partial failures are reported, not silently swallowed
  async function handleBulkDelete() {
    const results = await Promise.allSettled(selectedIds.map(id => deleteTemplate.mutateAsync({ id })));
    const failed = results.filter(r => r.status === 'rejected').length;
    if (failed > 0) {
      message.warning(t('common.bulkDeletePartialFail', { failed, total: selectedIds.length, defaultValue: `${failed}/${selectedIds.length} mục xóa thất bại` }));
    } else {
      message.success(t('common.bulkDeleteSuccess', { defaultValue: 'Xóa thành công' }));
    }
    setSelectedIds([]);
  }

  const handleCreate = async () => {
    const values = await form.validateFields();
    try {
      await createTemplate.mutateAsync({
        name: values.name,
        nameVi: values.nameVi,
        code: values.code,
      });
      message.success(t('common.operationSuccess', { defaultValue: 'Thao tác thành công' }));
      form.resetFields();
      setModalOpen(false);
    } catch {
      message.error(t('common.operationFailed', { defaultValue: 'Thao tác thất bại' }));
    }
  };

  const columns: ColumnsType<FormTemplateDto> = [
    {
      title: t('page.forms.col.code'),
      dataIndex: 'code',
      key: 'code',
      width: 120,
      render: (v: string) => <code>{v}</code>,
    },
    { title: t('page.forms.col.name'), dataIndex: 'name', key: 'name', ellipsis: true },
    {
      title: t('page.forms.col.status'),
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: (v: FormStatus) => (
        <Tag color={STATUS_COLOR[v]}>{t(`page.forms.status.${v}`)}</Tag>
      ),
    },
    {
      title: t('page.forms.col.fieldCount'),
      key: 'fieldCount',
      width: 100,
      render: (_: unknown, r: FormTemplateDto) => r.fields?.length ?? 0,
    },
    {
      title: t('page.forms.col.submissionsCount'),
      dataIndex: 'submissionsCount',
      key: 'submissionsCount',
      width: 100,
    },
    {
      title: t('page.forms.col.createdAt'),
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 120,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
    {
      title: '',
      key: 'actions',
      width: 140,
      render: (_: unknown, r: FormTemplateDto) => (
        <Space size="small">
          <Tooltip title={t('common.edit', 'Chỉnh sửa')}>
            <Button size="small" icon={<EditOutlined />} onClick={() => navigate({ to: `/forms/${r.id}` })} />
          </Tooltip>
          {r.status === 'Draft' && (
            <Popconfirm
              title={t('forms.deleteConfirm')}
              onConfirm={() => deleteTemplate.mutate({ id: r.id }, {
                onSuccess: () => message.success(t('common.deleteSuccess', { defaultValue: 'Xóa thành công' })),
                onError: () => message.error(t('common.deleteFailed', { defaultValue: 'Xóa thất bại' })),
              })}
              okText={t('common.confirm')}
              cancelText={t('common.cancel')}
            >
              <Button
                danger
                size="small"
                icon={<DeleteOutlined />}
                loading={deleteTemplate.isPending}
              />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <AdminPageHeader
        title={t('page.forms.title')}
        stats={{ total: data?.items?.length ?? 0, label: t('common.items') }}
        actions={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>
            {t('forms.createTemplate')}
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
        <Table<FormTemplateDto>
          rowKey="id"
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
          pagination={{ pageSize: 20, showSizeChanger: true, showTotal: (n) => t('page.forms.showTotal', { count: n }) }}
          locale={{ emptyText: t('page.forms.empty') }}
        />
      </AdminContentCard>

      <Modal
        title={t('forms.createTemplate')}
        open={modalOpen}
        onOk={handleCreate}
        onCancel={() => { setModalOpen(false); form.resetFields(); }}
        confirmLoading={createTemplate.isPending}
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" style={{ marginTop: 8 }}>
          <Form.Item
            name="code"
            label="Code"
            rules={[{ required: true, message: 'Code is required' }]}
          >
            <Input placeholder="e.g. FORM-001" />
          </Form.Item>
          <Form.Item
            name="name"
            label={t('forms.templateName') + ' (EN)'}
            rules={[{ required: true }]}
          >
            <Input />
          </Form.Item>
          <Form.Item
            name="nameVi"
            label={t('forms.templateName') + ' (VI)'}
            rules={[{ required: true }]}
          >
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
