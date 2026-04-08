import { useState } from 'react';
import { Table, Button, Space, Row, Col, Form, Popconfirm, Card, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { MoneyInput, DatePickerMaxToday } from '@/features/shared/components';
import { useAddPppDisbursement, useDeletePppDisbursement, usePppProject } from '../ppp-project-api';
import type { PppDisbursementRecordDto } from '../ppp-project-types';

interface Tab4Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

// PppTab4Disbursement — 3-source disbursement tracking: Vốn NN / Vốn CSH / Vốn vay.
// Each row has period + cumulative for each source. Cumulative is entered by user (API stores it).
export function PppTab4Disbursement({ projectId, mode, onSaved: _onSaved }: Tab4Props) {
  const isReadonly = mode === 'detail';
  const { data: project } = usePppProject(projectId);
  const disbursements = project?.disbursements ?? [];

  const [form] = Form.useForm();
  const addMutation = useAddPppDisbursement();
  const deleteMutation = useDeletePppDisbursement();

  async function handleAdd() {
    const values = await form.validateFields();
    addMutation.mutate(
      { projectId, ...values, reportDate: values.reportDate?.format('YYYY-MM-DD') },
      {
        onSuccess: () => { message.success('Thêm giải ngân thành công'); form.resetFields(); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  function handleDelete(disbursementId: string) {
    deleteMutation.mutate({ projectId, disbursementId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<PppDisbursementRecordDto> = [
    {
      title: 'Kỳ báo cáo',
      dataIndex: 'reportDate',
      key: 'reportDate',
      width: 120,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
    {
      title: 'Vốn NN – Kỳ này',
      dataIndex: 'vonNNPeriod',
      key: 'vonNNPeriod',
      width: 140,
      render: (v: number) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: 'Vốn NN – Lũy kế',
      dataIndex: 'vonNNCumulative',
      key: 'vonNNCumulative',
      width: 140,
      render: (v: number) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: 'Vốn CSH – Kỳ này',
      dataIndex: 'vonCSHPeriod',
      key: 'vonCSHPeriod',
      width: 140,
      render: (v: number) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: 'Vốn CSH – Lũy kế',
      dataIndex: 'vonCSHCumulative',
      key: 'vonCSHCumulative',
      width: 140,
      render: (v: number) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: 'Vốn vay – Kỳ này',
      dataIndex: 'vonVayPeriod',
      key: 'vonVayPeriod',
      width: 140,
      render: (v: number) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: 'Vốn vay – Lũy kế',
      dataIndex: 'vonVayCumulative',
      key: 'vonVayCumulative',
      width: 140,
      render: (v: number) => v?.toLocaleString('vi-VN') ?? '—',
    },
    {
      title: '',
      key: 'actions',
      width: 50,
      render: (_, r) =>
        !isReadonly && (
          <Popconfirm
            title="Xóa bản ghi giải ngân?"
            onConfirm={() => handleDelete(r.id)}
            okText="Xóa"
            cancelText="Hủy"
          >
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        ),
    },
  ];

  return (
    <Card size="small" title="Giải ngân dự án PPP">
      {!isReadonly && (
        <Form form={form} layout="vertical" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={4}>
              <Form.Item name="reportDate" label="Kỳ báo cáo" rules={[{ required: true, message: 'Bắt buộc' }]}>
                <DatePickerMaxToday size="small" style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={3}>
              <Form.Item name="vonNNPeriod" label="Vốn NN – Kỳ">
                <MoneyInput size="small" />
              </Form.Item>
            </Col>
            <Col span={3}>
              <Form.Item name="vonNNCumulative" label="Vốn NN – LK">
                <MoneyInput size="small" />
              </Form.Item>
            </Col>
            <Col span={3}>
              <Form.Item name="vonCSHPeriod" label="CSH – Kỳ">
                <MoneyInput size="small" />
              </Form.Item>
            </Col>
            <Col span={3}>
              <Form.Item name="vonCSHCumulative" label="CSH – LK">
                <MoneyInput size="small" />
              </Form.Item>
            </Col>
            <Col span={3}>
              <Form.Item name="vonVayPeriod" label="Vay – Kỳ">
                <MoneyInput size="small" />
              </Form.Item>
            </Col>
            <Col span={3}>
              <Form.Item name="vonVayCumulative" label="Vay – LK">
                <MoneyInput size="small" />
              </Form.Item>
            </Col>
          </Row>
          <Button size="small" icon={<PlusOutlined />} onClick={handleAdd} loading={addMutation.isPending}>
            Thêm bản ghi giải ngân
          </Button>
        </Form>
      )}
      <Table<PppDisbursementRecordDto>
        rowKey="id"
        columns={columns}
        dataSource={disbursements}
        size="small"
        scroll={{ x: 1100 }}
        pagination={{ pageSize: 10, showTotal: (t) => `${t} bản ghi` }}
        locale={{ emptyText: 'Chưa có dữ liệu giải ngân' }}
      />
    </Card>
  );
}
