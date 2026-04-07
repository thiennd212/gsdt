import { useState } from 'react';
import {
  Table, Button, Space, Popconfirm, Tag,
  Modal, Form, Input, InputNumber, Switch, Divider, message, Flex,
} from 'antd';
import { PlusOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import {
  useCredentialPolicies,
  useCreateCredentialPolicy,
  useUpdateCredentialPolicy,
  useDeleteCredentialPolicy,
  type CredentialPolicyDto,
  type CreateCredentialPolicyDto,
} from './credential-policies-api';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';

// CredentialPoliciesPage — manage password rules, lockout, and history policies
export function CredentialPoliciesPage() {
  const { t } = useTranslation();
  const [modalOpen, setModalOpen] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [editingPolicy, setEditingPolicy] = useState<CredentialPolicyDto | null>(null);
  const [form] = Form.useForm<CreateCredentialPolicyDto>();

  const { data, isLoading } = useCredentialPolicies();
  const createPolicy = useCreateCredentialPolicy();
  const updatePolicy = useUpdateCredentialPolicy();
  const deletePolicy = useDeleteCredentialPolicy();

  const policies = data?.items ?? [];
  const total = data?.totalCount ?? 0;

  const columns: ColumnsType<CredentialPolicyDto> = [
    { title: 'Tên chính sách', dataIndex: 'name', key: 'name', ellipsis: true },
    { title: 'Ký tự tối thiểu', dataIndex: 'minLength', key: 'minLength', width: 130 },
    { title: 'Hạn đổi MK (ngày)', dataIndex: 'rotationDays', key: 'rotationDays', width: 160 },
    { title: 'Thử sai tối đa', dataIndex: 'maxFailedAttempts', key: 'maxFailedAttempts', width: 130 },
    { title: 'Khóa TK (phút)', dataIndex: 'lockoutMinutes', key: 'lockoutMinutes', width: 130 },
    {
      title: 'Mặc định',
      dataIndex: 'isDefault',
      key: 'isDefault',
      width: 100,
      render: (v: boolean) => v ? <Tag color="green">Mặc định</Tag> : null,
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
            onClick={() => { setEditingPolicy(record); form.setFieldsValue(record); setModalOpen(true); }}
          />
          <Popconfirm
            title="Xóa chính sách này?"
            onConfirm={() => deletePolicy.mutate(record.id, {
              onSuccess: () => message.success(t('common.deleteSuccess', { defaultValue: 'Xóa thành công' })),
              onError: () => message.error(t('common.deleteFailed', { defaultValue: 'Xóa thất bại' })),
            })}
            okText={t('common.confirm', 'Xác nhận')}
            cancelText={t('common.cancel', 'Hủy')}
          >
            <Button danger icon={<DeleteOutlined />} size="small" loading={deletePolicy.isPending} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  async function handleBulkDelete() {
    const results = await Promise.allSettled(selectedIds.map((id) => deletePolicy.mutateAsync(id)));
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
      if (editingPolicy) {
        await updatePolicy.mutateAsync({ ...values, id: editingPolicy.id });
      } else {
        await createPolicy.mutateAsync(values);
      }
      message.success(t('common.operationSuccess', { defaultValue: 'Thao tác thành công' }));
      form.resetFields();
      setEditingPolicy(null);
      setModalOpen(false);
    } catch {
      message.error(t('common.operationFailed', { defaultValue: 'Thao tác thất bại' }));
    }
  };

  return (
    <div>
      <AdminPageHeader
        title="Chính sách mật khẩu"
        stats={{ total, label: 'chính sách' }}
        actions={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditingPolicy(null); form.resetFields(); setModalOpen(true); }}>
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
        <Table<CredentialPolicyDto>
          rowKey="id"
          rowSelection={{
            selectedRowKeys: selectedIds,
            onChange: (keys) => setSelectedIds(keys as string[]),
          }}
          columns={columns}
          dataSource={policies.filter(item =>
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
        title={editingPolicy ? 'Sửa chính sách mật khẩu' : 'Tạo chính sách mật khẩu'}
        open={modalOpen}
        onOk={handleSave}
        onCancel={() => { setModalOpen(false); setEditingPolicy(null); form.resetFields(); }}
        confirmLoading={createPolicy.isPending || updatePolicy.isPending}
        okText={t('common.save', 'Lưu')}
        cancelText={t('common.cancel', 'Hủy')}
        destroyOnHidden
        width={520}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 8 }}>
          <Form.Item name="name" label="Tên chính sách" rules={[{ required: true, message: 'Vui lòng nhập tên' }]}>
            <Input />
          </Form.Item>

          <Divider orientation="left" orientationMargin={0}>Quy tắc mật khẩu</Divider>

          <Space style={{ width: '100%' }} wrap>
            <Form.Item name="minLength" label="Độ dài tối thiểu" rules={[{ required: true }]} initialValue={8}>
              <InputNumber min={4} max={128} style={{ width: 120 }} />
            </Form.Item>
            <Form.Item name="maxLength" label="Độ dài tối đa" rules={[{ required: true }]} initialValue={128}>
              <InputNumber min={4} max={256} style={{ width: 120 }} />
            </Form.Item>
          </Space>
          <Space style={{ width: '100%' }} wrap>
            <Form.Item name="requireUppercase" label="Yêu cầu chữ hoa" valuePropName="checked" initialValue={false}>
              <Switch />
            </Form.Item>
            <Form.Item name="requireLowercase" label="Yêu cầu chữ thường" valuePropName="checked" initialValue={false}>
              <Switch />
            </Form.Item>
            <Form.Item name="requireDigit" label="Yêu cầu chữ số" valuePropName="checked" initialValue={false}>
              <Switch />
            </Form.Item>
            <Form.Item name="requireSpecialChar" label="Ký tự đặc biệt" valuePropName="checked" initialValue={false}>
              <Switch />
            </Form.Item>
          </Space>

          <Divider orientation="left" orientationMargin={0}>Khóa tài khoản</Divider>

          <Form.Item name="maxFailedAttempts" label="Số lần thử tối đa" rules={[{ required: true }]} initialValue={5}>
            <InputNumber min={1} max={100} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="lockoutMinutes" label="Thời gian khóa (phút)" rules={[{ required: true }]} initialValue={15}>
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>

          <Divider orientation="left" orientationMargin={0}>Lịch sử mật khẩu</Divider>

          <Form.Item name="rotationDays" label="Xoay mật khẩu sau (ngày)" rules={[{ required: true }]} initialValue={90}>
            <InputNumber min={0} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="passwordHistoryCount" label="Số mật khẩu cũ lưu lại" rules={[{ required: true }]} initialValue={5}>
            <InputNumber min={0} max={24} style={{ width: '100%' }} />
          </Form.Item>

          <Divider orientation="left" orientationMargin={0}>Cài đặt khác</Divider>

          <Form.Item name="isDefault" label="Đặt làm mặc định" valuePropName="checked" initialValue={false}>
            <Switch />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
