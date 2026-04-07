import { useState } from 'react';
import {
  Table, Button, Space, Popconfirm, Tag,
  Modal, Form, Select, Input, Switch, message, Flex,
} from 'antd';
import { PlusOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import {
  useSodRules,
  useCreateSodRule,
  useUpdateSodRule,
  useDeleteSodRule,
  type SodRuleDto,
  type CreateSodRuleDto,
  type UpdateSodRuleDto,
} from './sod-rules-api';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';

// Enforcement level tag colors — escalating impact
const ENFORCEMENT_COLORS: Record<string, string> = {
  Warning: 'orange',
  Block:   'red',
  Audit:   'blue',
};

const ENFORCEMENT_OPTIONS = ['Warning', 'Block', 'Audit'].map((v) => ({ value: v, label: v }));

// SodRulesPage — manage Segregation of Duties conflict rules between permission pairs
export function SodRulesPage() {
  const { t } = useTranslation();
  const [modalOpen, setModalOpen] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [editingRule, setEditingRule] = useState<SodRuleDto | null>(null);
  const [form] = Form.useForm<CreateSodRuleDto | UpdateSodRuleDto>();

  const { data, isLoading } = useSodRules();
  const createRule = useCreateSodRule();
  const updateRule = useUpdateSodRule();
  const deleteRule = useDeleteSodRule();

  const rules = data ?? [];
  const total = rules.length;

  const columns: ColumnsType<SodRuleDto> = [
    {
      title: 'Quyền A',
      dataIndex: 'permissionCodeA',
      key: 'permissionCodeA',
      width: 170,
      render: (v: string) => <Tag>{v}</Tag>,
    },
    {
      title: 'Quyền B',
      dataIndex: 'permissionCodeB',
      key: 'permissionCodeB',
      width: 170,
      render: (v: string) => <Tag>{v}</Tag>,
    },
    {
      title: 'Mức thực thi',
      dataIndex: 'enforcementLevel',
      key: 'enforcementLevel',
      width: 120,
      render: (v: string) => <Tag color={ENFORCEMENT_COLORS[v] ?? 'default'}>{v}</Tag>,
    },
    {
      title: 'Mô tả',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (v?: string) => v ?? '—',
    },
    {
      title: 'Kích hoạt',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 90,
      render: (v: boolean) =>
        v ? <Tag color="green">Bật</Tag> : <Tag color="default">Tắt</Tag>,
    },
    {
      title: '',
      key: 'actions',
      width: 80,
      render: (_, record) => (
        <Space size="small">
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => { setEditingRule(record); form.setFieldsValue(record); setModalOpen(true); }}
          />
          <Popconfirm
            title="Xóa quy tắc SoD này?"
            onConfirm={() => deleteRule.mutate(record.id, {
              onSuccess: () => message.success(t('common.deleteSuccess', { defaultValue: 'Xóa thành công' })),
              onError: () => message.error(t('common.deleteFailed', { defaultValue: 'Xóa thất bại' })),
            })}
            okText={t('common.confirm', 'Xác nhận')}
            cancelText={t('common.cancel', 'Hủy')}
          >
            <Button danger icon={<DeleteOutlined />} size="small" loading={deleteRule.isPending} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  async function handleBulkDelete() {
    const results = await Promise.allSettled(selectedIds.map((id) => deleteRule.mutateAsync(id)));
    const failed = results.filter(r => r.status === 'rejected').length;
    if (failed > 0) {
      message.warning(t('common.bulkDeletePartialFail', { failed, total: selectedIds.length, defaultValue: `${failed}/${selectedIds.length} mục xóa thất bại` }));
    } else {
      message.success(t('common.bulkDeleteSuccess', { defaultValue: `Đã xóa ${selectedIds.length} mục` }));
    }
    setSelectedIds([]);
  }

  const handleSave = async () => {
    const values = await form.validateFields();
    try {
      if (editingRule) {
        await updateRule.mutateAsync({ ...(values as UpdateSodRuleDto), id: editingRule.id });
      } else {
        await createRule.mutateAsync(values as CreateSodRuleDto);
      }
      message.success(t('common.operationSuccess', { defaultValue: 'Thao tác thành công' }));
      form.resetFields();
      setEditingRule(null);
      setModalOpen(false);
    } catch {
      message.error(t('common.operationFailed', { defaultValue: 'Thao tác thất bại' }));
    }
  };

  return (
    <div>
      <AdminPageHeader
        title="Quy tắc phân tách nhiệm vụ (SoD)"
        stats={{ total, label: 'quy tắc' }}
        actions={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => { setEditingRule(null); form.resetFields(); setModalOpen(true); }}
          >
            {t('common.add', 'Thêm')}
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
            >
              <Button danger size="small">
                {t('common.deleteSelected', { defaultValue: `Xóa (${selectedIds.length})` })}
              </Button>
            </Popconfirm>
          </Flex>
        )}
        <Table<SodRuleDto>
          rowKey="id"
          rowSelection={{
            selectedRowKeys: selectedIds,
            onChange: (keys) => setSelectedIds(keys as string[]),
          }}
          columns={columns}
          dataSource={rules.filter(item =>
            !searchText || Object.values(item).some(v =>
              String(v ?? '').toLowerCase().includes(searchText.toLowerCase())
            )
          )}
          loading={isLoading}
          size="small"
          pagination={{ pageSize: 20, showSizeChanger: true, showTotal: (t: number) => `Tổng ${t}` }}
        />
      </AdminContentCard>

      <Modal
        title={editingRule ? 'Sửa quy tắc SoD' : 'Tạo quy tắc SoD'}
        open={modalOpen}
        onOk={handleSave}
        onCancel={() => { setModalOpen(false); setEditingRule(null); form.resetFields(); }}
        confirmLoading={createRule.isPending || updateRule.isPending}
        okText={t('common.save', 'Lưu')}
        cancelText={t('common.cancel', 'Hủy')}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" style={{ marginTop: 8 }}>
          <Form.Item name="permissionCodeA" label="Mã quyền A" rules={[{ required: true, message: 'Vui lòng nhập mã quyền A' }]}>
            <Input placeholder="VD: cases.approve" />
          </Form.Item>
          <Form.Item name="permissionCodeB" label="Mã quyền B" rules={[{ required: true, message: 'Vui lòng nhập mã quyền B' }]}>
            <Input placeholder="VD: cases.create" />
          </Form.Item>
          <Form.Item name="enforcementLevel" label="Mức thực thi" rules={[{ required: true, message: 'Vui lòng chọn mức thực thi' }]} initialValue="Warning">
            <Select options={ENFORCEMENT_OPTIONS} />
          </Form.Item>
          <Form.Item name="description" label="Mô tả">
            <Input.TextArea rows={3} placeholder="Giải thích lý do xung đột..." />
          </Form.Item>
          {/* isActive only shown in edit mode */}
          {editingRule && (
            <Form.Item name="isActive" label="Kích hoạt" valuePropName="checked">
              <Switch />
            </Form.Item>
          )}
        </Form>
      </Modal>
    </div>
  );
}
