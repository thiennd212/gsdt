import { useState } from 'react';
import { Table, Button, Space, Row, Col, Form, Popconfirm, Card, Select, Input, Checkbox, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { MoneyInput } from '@/features/shared/components';
import { useAddRevenueReport, useDeleteRevenueReport, usePppProject } from '../ppp-project-api';
import type { RevenueReportDto } from '../ppp-project-types';

interface Tab6Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

const YEAR_OPTIONS = Array.from({ length: 30 }, (_, i) => {
  const y = new Date().getFullYear() - 10 + i;
  return { value: y, label: String(y) };
});

const PERIOD_OPTIONS = [
  { value: 1, label: '6 tháng đầu năm' },
  { value: 2, label: '6 tháng cuối năm' },
  { value: 3, label: 'Cả năm' },
];

// PppTab6OperationRevenue — settlement checkboxes + revenue report table.
// Revenue table: Year, Period, RevenuePeriod, Cumulative (auto-calc), Sharing change, Difficulties.
export function PppTab6OperationRevenue({ projectId, mode, onSaved: _onSaved }: Tab6Props) {
  const isReadonly = mode === 'detail';
  const { data: project } = usePppProject(projectId);
  const revenueReports = project?.revenueReports ?? [];

  const [addForm] = Form.useForm();
  const addMutation = useAddRevenueReport();
  const deleteMutation = useDeleteRevenueReport();

  // Auto-calc cumulative from sorted reports — display only in table
  function calcCumulative(report: RevenueReportDto): number {
    const sorted = [...revenueReports].sort((a, b) => a.year - b.year || a.period - b.period);
    let cum = 0;
    for (const r of sorted) {
      cum += r.revenuePeriod;
      if (r.id === report.id) break;
    }
    return cum;
  }

  async function handleAdd() {
    const values = await addForm.validateFields();
    addMutation.mutate(
      { projectId, ...values },
      {
        onSuccess: () => { message.success('Thêm báo cáo DT thành công'); addForm.resetFields(); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  function handleDelete(reportId: string) {
    deleteMutation.mutate({ projectId, reportId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<RevenueReportDto> = [
    {
      title: 'Năm',
      dataIndex: 'year',
      key: 'year',
      width: 80,
    },
    {
      title: 'Kỳ',
      dataIndex: 'period',
      key: 'period',
      width: 160,
      render: (v: number) => PERIOD_OPTIONS.find((o) => o.value === v)?.label ?? v,
    },
    {
      title: 'DT kỳ (triệu VNĐ)',
      dataIndex: 'revenuePeriod',
      key: 'revenuePeriod',
      width: 150,
      render: (v: number) => v?.toLocaleString('vi-VN'),
    },
    {
      title: 'Lũy kế (tự tính)',
      key: 'cumulative',
      width: 140,
      render: (_, r) => calcCumulative(r).toLocaleString('vi-VN'),
    },
    {
      title: 'Tăng/giảm chia sẻ',
      dataIndex: 'sharingChange',
      key: 'sharingChange',
      width: 150,
      render: (v) => v ?? '—',
    },
    {
      title: 'Khó khăn',
      dataIndex: 'difficulties',
      key: 'difficulties',
      ellipsis: true,
      render: (v) => v ?? '—',
    },
    {
      title: 'Kiến nghị',
      dataIndex: 'recommendations',
      key: 'recommendations',
      ellipsis: true,
      render: (v) => v ?? '—',
    },
    {
      title: '',
      key: 'actions',
      width: 50,
      render: (_, r) =>
        !isReadonly && (
          <Popconfirm
            title="Xóa báo cáo doanh thu?"
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
    <div>
      {/* Settlement checkboxes */}
      <Card size="small" title="Quyết toán" style={{ marginBottom: 16 }}>
        <Space direction="vertical">
          <Checkbox disabled={isReadonly}>Đã quyết toán hoàn thành</Checkbox>
          <Checkbox disabled={isReadonly}>Đã phê duyệt quyết toán</Checkbox>
          <Checkbox disabled={isReadonly}>Đang trong quá trình quyết toán</Checkbox>
          <Checkbox disabled={isReadonly}>Chưa thực hiện quyết toán</Checkbox>
        </Space>
      </Card>

      {/* Revenue reports */}
      <Card size="small" title="Báo cáo doanh thu khai thác">
        {!isReadonly && (
          <Form form={addForm} layout="vertical" style={{ marginBottom: 16 }}>
            <Row gutter={16}>
              <Col span={4}>
                <Form.Item name="year" label="Năm" rules={[{ required: true, message: 'Bắt buộc' }]}>
                  <Select options={YEAR_OPTIONS} placeholder="Năm" size="small" />
                </Form.Item>
              </Col>
              <Col span={5}>
                <Form.Item name="period" label="Kỳ báo cáo" rules={[{ required: true, message: 'Bắt buộc' }]}>
                  <Select options={PERIOD_OPTIONS} placeholder="Kỳ" size="small" />
                </Form.Item>
              </Col>
              <Col span={5}>
                <Form.Item name="revenuePeriod" label="DT kỳ này" rules={[{ required: true }]}>
                  <MoneyInput size="small" />
                </Form.Item>
              </Col>
              <Col span={5}>
                <Form.Item name="sharingChange" label="Tăng/giảm chia sẻ">
                  <Input size="small" placeholder="Mô tả" />
                </Form.Item>
              </Col>
              <Col span={5}>
                <Form.Item name="difficulties" label="Khó khăn">
                  <Input size="small" placeholder="Khó khăn" />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item name="recommendations" label="Kiến nghị">
                  <Input size="small" placeholder="Kiến nghị" />
                </Form.Item>
              </Col>
            </Row>
            <Button size="small" icon={<PlusOutlined />} onClick={handleAdd} loading={addMutation.isPending}>
              Thêm báo cáo DT
            </Button>
          </Form>
        )}
        <Table<RevenueReportDto>
          rowKey="id"
          columns={columns}
          dataSource={revenueReports}
          size="small"
          scroll={{ x: 1000 }}
          pagination={{ pageSize: 10, showTotal: (t) => `${t} bản ghi` }}
          locale={{ emptyText: 'Chưa có dữ liệu doanh thu' }}
        />
      </Card>
    </div>
  );
}
