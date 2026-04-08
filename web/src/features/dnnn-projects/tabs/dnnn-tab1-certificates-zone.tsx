import { useState } from 'react';
import { Table, Button, Space, Popconfirm, Form, Input, InputNumber, Row, Col, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined, EditOutlined, SaveOutlined, CloseOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { DatePickerMaxToday } from '@/features/shared/components';
import {
  useAddDnnnCertificate,
  useUpdateDnnnCertificate,
  useDeleteDnnnCertificate,
  useDnnnProject,
} from '../dnnn-project-api';
import type { RegistrationCertificateDto } from '../dnnn-project-types';

interface CertificatesZoneProps {
  projectId: string;
  disabled?: boolean;
}

type AddFormValues = {
  certificateNumber: string;
  issuedDate: dayjs.Dayjs | null;
  investmentCapital: number;
  equityCapital: number;
  notes: string;
};

// DnnnTab1CertificatesZone — inline CRUD for GCNĐKĐT (registration certificates).
// Add form at top; table below with inline edit (row-level) and delete actions.
export function DnnnTab1CertificatesZone({ projectId, disabled }: CertificatesZoneProps) {
  const { data: project } = useDnnnProject(projectId);
  const certificates = project?.certificates ?? [];

  const [addForm] = Form.useForm<AddFormValues>();
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editForm] = Form.useForm<AddFormValues>();

  const addMutation = useAddDnnnCertificate();
  const updateMutation = useUpdateDnnnCertificate();
  const deleteMutation = useDeleteDnnnCertificate();

  async function handleAdd() {
    const values = await addForm.validateFields();
    addMutation.mutate(
      {
        projectId,
        ...values,
        issuedDate: values.issuedDate?.format('YYYY-MM-DD'),
      },
      {
        onSuccess: () => { message.success('Thêm GCNĐKĐT thành công'); addForm.resetFields(); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  function startEdit(record: RegistrationCertificateDto) {
    setEditingId(record.id);
    editForm.setFieldsValue({
      certificateNumber: record.certificateNumber,
      issuedDate: record.issuedDate ? dayjs(record.issuedDate) : null,
      investmentCapital: record.investmentCapital,
      equityCapital: record.equityCapital,
      notes: record.notes ?? '',
    });
  }

  async function handleUpdate(certificateId: string) {
    const values = await editForm.validateFields();
    updateMutation.mutate(
      {
        projectId,
        certificateId,
        ...values,
        issuedDate: values.issuedDate?.format('YYYY-MM-DD'),
      },
      {
        onSuccess: () => { message.success('Cập nhật thành công'); setEditingId(null); },
        onError: () => message.error('Cập nhật thất bại'),
      },
    );
  }

  function handleDelete(certificateId: string) {
    deleteMutation.mutate({ projectId, certificateId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<RegistrationCertificateDto> = [
    {
      title: 'Số GCNĐKĐT',
      dataIndex: 'certificateNumber',
      key: 'number',
      width: 160,
      render: (v, r) =>
        editingId === r.id ? (
          <Form form={editForm} component={false}>
            <Form.Item name="certificateNumber" style={{ margin: 0 }} rules={[{ required: true }]}>
              <Input size="small" />
            </Form.Item>
          </Form>
        ) : v,
    },
    {
      title: 'Ngày cấp',
      dataIndex: 'issuedDate',
      key: 'date',
      width: 120,
      render: (v, r) =>
        editingId === r.id ? (
          <Form form={editForm} component={false}>
            <Form.Item name="issuedDate" style={{ margin: 0 }}>
              <DatePickerMaxToday size="small" style={{ width: '100%' }} />
            </Form.Item>
          </Form>
        ) : (v ? dayjs(v).format('DD/MM/YYYY') : '—'),
    },
    {
      title: 'Vốn ĐT (tr.đ)',
      dataIndex: 'investmentCapital',
      key: 'invest',
      width: 140,
      render: (v, r) =>
        editingId === r.id ? (
          <Form form={editForm} component={false}>
            <Form.Item name="investmentCapital" style={{ margin: 0 }}>
              <InputNumber size="small" min={0} style={{ width: '100%' }} />
            </Form.Item>
          </Form>
        ) : v?.toLocaleString('vi-VN'),
    },
    {
      title: 'Vốn CSH (tr.đ)',
      dataIndex: 'equityCapital',
      key: 'equity',
      width: 140,
      render: (v, r) =>
        editingId === r.id ? (
          <Form form={editForm} component={false}>
            <Form.Item name="equityCapital" style={{ margin: 0 }}>
              <InputNumber size="small" min={0} style={{ width: '100%' }} />
            </Form.Item>
          </Form>
        ) : v?.toLocaleString('vi-VN'),
    },
    {
      title: 'Tỷ lệ CSH (%)',
      dataIndex: 'equityRatio',
      key: 'ratio',
      width: 120,
      render: (v: number | null) => (v != null ? `${v}%` : '—'),
    },
    {
      title: 'Ghi chú',
      dataIndex: 'notes',
      key: 'notes',
      ellipsis: true,
      render: (v, r) =>
        editingId === r.id ? (
          <Form form={editForm} component={false}>
            <Form.Item name="notes" style={{ margin: 0 }}>
              <Input size="small" />
            </Form.Item>
          </Form>
        ) : (v ?? '—'),
    },
    {
      title: '',
      key: 'actions',
      width: 90,
      render: (_, r) => {
        if (disabled) return null;
        if (editingId === r.id) {
          return (
            <Space size="small">
              <Button size="small" icon={<SaveOutlined />} onClick={() => handleUpdate(r.id)} loading={updateMutation.isPending} />
              <Button size="small" icon={<CloseOutlined />} onClick={() => setEditingId(null)} />
            </Space>
          );
        }
        return (
          <Space size="small">
            <Button size="small" icon={<EditOutlined />} onClick={() => startEdit(r)} />
            <Popconfirm title="Xóa GCNĐKĐT?" onConfirm={() => handleDelete(r.id)} okText="Xóa" cancelText="Hủy">
              <Button size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          </Space>
        );
      },
    },
  ];

  return (
    <div>
      {/* Add form */}
      {!disabled && (
        <Form form={addForm} layout="vertical" style={{ marginBottom: 12 }}>
          <Row gutter={12}>
            <Col span={5}>
              <Form.Item name="certificateNumber" label="Số GCNĐKĐT" rules={[{ required: true, message: 'Bắt buộc' }]}>
                <Input size="small" placeholder="Số GCN" />
              </Form.Item>
            </Col>
            <Col span={4}>
              <Form.Item name="issuedDate" label="Ngày cấp">
                <DatePickerMaxToday size="small" style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={4}>
              <Form.Item name="investmentCapital" label="Vốn ĐT (tr.đ)">
                <InputNumber size="small" min={0} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={4}>
              <Form.Item name="equityCapital" label="Vốn CSH (tr.đ)">
                <InputNumber size="small" min={0} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={5}>
              <Form.Item name="notes" label="Ghi chú">
                <Input size="small" placeholder="Ghi chú" />
              </Form.Item>
            </Col>
            <Col span={2} style={{ display: 'flex', alignItems: 'flex-end', paddingBottom: 24 }}>
              <Button size="small" icon={<PlusOutlined />} onClick={handleAdd} loading={addMutation.isPending}>
                Thêm
              </Button>
            </Col>
          </Row>
        </Form>
      )}

      <Table<RegistrationCertificateDto>
        rowKey="id"
        columns={columns}
        dataSource={certificates}
        size="small"
        pagination={{ pageSize: 5, showTotal: (t) => `${t} GCN` }}
        locale={{ emptyText: 'Chưa có GCNĐKĐT' }}
      />
    </div>
  );
}
