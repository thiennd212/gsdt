import { useState } from 'react';
import { Table, Button, Space, Input, Row, Col, Form, Popconfirm, Select, Card, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { MoneyInput, DatePickerMaxToday, FileUploadField } from '@/features/shared/components';
import {
  useAddPppCapitalPlan,
  useDeletePppCapitalPlan,
  useAddPppExecutionRecord,
  usePppProject,
} from '../ppp-project-api';
import type { PppCapitalPlanDto, PppExecutionRecordDto } from '../ppp-project-types';

interface Tab3Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

const DECISION_TYPE_OPTIONS = [
  { value: 0, label: 'Ban đầu' },
  { value: 1, label: 'Điều chỉnh' },
];

const PROGRESS_STATUS_OPTIONS = [
  { value: 1, label: 'Đúng tiến độ' },
  { value: 2, label: 'Chậm tiến độ' },
  { value: 3, label: 'Vượt tiến độ' },
  { value: 4, label: 'Tạm dừng' },
];

// PppTab3Implementation — capital plans, execution records for PPP projects.
export function PppTab3Implementation({ projectId, mode, onSaved: _onSaved }: Tab3Props) {
  const isReadonly = mode === 'detail';
  const { data: project } = usePppProject(projectId);

  const [planForm] = Form.useForm();
  const [execForm] = Form.useForm();

  const addPlanMutation = useAddPppCapitalPlan();
  const deletePlanMutation = useDeletePppCapitalPlan();
  const addExecMutation = useAddPppExecutionRecord();

  const capitalPlans = project?.capitalPlans ?? [];
  const executionRecords = project?.executionRecords ?? [];

  async function handleAddPlan() {
    const values = await planForm.validateFields();
    addPlanMutation.mutate(
      { projectId, ...values, decisionDate: values.decisionDate?.format('YYYY-MM-DD') },
      {
        onSuccess: () => { message.success('Thêm kế hoạch vốn thành công'); planForm.resetFields(); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  function handleDeletePlan(planId: string) {
    deletePlanMutation.mutate({ projectId, planId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  async function handleAddExec() {
    const values = await execForm.validateFields();
    addExecMutation.mutate(
      { projectId, ...values, reportDate: values.reportDate?.format('YYYY-MM-DD') },
      {
        onSuccess: () => { message.success('Thêm hồ sơ TH thành công'); execForm.resetFields(); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  const planColumns: ColumnsType<PppCapitalPlanDto> = [
    { title: 'Loại QĐ', dataIndex: 'decisionType', key: 'type', width: 120,
      render: (v: number) => DECISION_TYPE_OPTIONS.find((o) => o.value === v)?.label ?? v },
    { title: 'Số QĐ', dataIndex: 'decisionNumber', key: 'number', width: 130 },
    { title: 'Ngày QĐ', dataIndex: 'decisionDate', key: 'date', width: 110,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Tổng vốn (triệu VNĐ)', dataIndex: 'totalAmount', key: 'amount', width: 160,
      render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'Ghi chú', dataIndex: 'notes', key: 'notes', ellipsis: true, render: (v) => v ?? '—' },
    {
      title: '', key: 'actions', width: 50,
      render: (_, r) => !isReadonly && (
        <Popconfirm title="Xóa kế hoạch vốn?" onConfirm={() => handleDeletePlan(r.id)} okText="Xóa" cancelText="Hủy">
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  const execColumns: ColumnsType<PppExecutionRecordDto> = [
    { title: 'Ngày báo cáo', dataIndex: 'reportDate', key: 'date', width: 120,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Tình trạng', dataIndex: 'progressStatus', key: 'status', width: 140,
      render: (v: number) => PROGRESS_STATUS_OPTIONS.find((o) => o.value === v)?.label ?? v },
    { title: 'TL hoàn thành (%)', dataIndex: 'physicalProgressPercent', key: 'pct', width: 140,
      render: (v: number | null) => v != null ? `${v}%` : '—' },
    { title: 'Ghi chú', dataIndex: 'notes', key: 'notes', ellipsis: true, render: (v) => v ?? '—' },
  ];

  return (
    <div>
      {/* Capital plans */}
      <Card size="small" title="Kế hoạch vốn" style={{ marginBottom: 16 }}>
        {!isReadonly && (
          <Form form={planForm} layout="vertical" style={{ marginBottom: 12 }}>
            <Row gutter={16}>
              <Col span={5}>
                <Form.Item name="decisionType" label="Loại QĐ" rules={[{ required: true }]}>
                  <Select options={DECISION_TYPE_OPTIONS} placeholder="Loại" size="small" />
                </Form.Item>
              </Col>
              <Col span={5}>
                <Form.Item name="decisionNumber" label="Số QĐ" rules={[{ required: true, message: 'Bắt buộc' }]}>
                  <Input size="small" placeholder="Số QĐ" />
                </Form.Item>
              </Col>
              <Col span={5}>
                <Form.Item name="decisionDate" label="Ngày QĐ">
                  <DatePickerMaxToday size="small" style={{ width: '100%' }} />
                </Form.Item>
              </Col>
              <Col span={5}>
                <Form.Item name="totalAmount" label="Tổng vốn" rules={[{ required: true }]}>
                  <MoneyInput size="small" />
                </Form.Item>
              </Col>
              <Col span={4}>
                <Form.Item name="fileId" label="Văn bản">
                  <FileUploadField accept=".pdf" maxCount={1} />
                </Form.Item>
              </Col>
            </Row>
            <Button size="small" icon={<PlusOutlined />} onClick={handleAddPlan} loading={addPlanMutation.isPending}>
              Thêm kế hoạch vốn
            </Button>
          </Form>
        )}
        <Table<PppCapitalPlanDto>
          rowKey="id"
          columns={planColumns}
          dataSource={capitalPlans}
          size="small"
          pagination={{ pageSize: 5 }}
          locale={{ emptyText: 'Chưa có kế hoạch vốn' }}
        />
      </Card>

      {/* Execution records */}
      <Card size="small" title="Hồ sơ thực hiện">
        {!isReadonly && (
          <Form form={execForm} layout="vertical" style={{ marginBottom: 12 }}>
            <Row gutter={16}>
              <Col span={6}>
                <Form.Item name="reportDate" label="Ngày báo cáo" rules={[{ required: true }]}>
                  <DatePickerMaxToday size="small" style={{ width: '100%' }} />
                </Form.Item>
              </Col>
              <Col span={6}>
                <Form.Item name="progressStatus" label="Tình trạng" rules={[{ required: true }]}>
                  <Select options={PROGRESS_STATUS_OPTIONS} placeholder="Tình trạng" size="small" />
                </Form.Item>
              </Col>
              <Col span={6}>
                <Form.Item name="physicalProgressPercent" label="TL hoàn thành (%)">
                  <Input type="number" min={0} max={100} size="small" placeholder="%" />
                </Form.Item>
              </Col>
              <Col span={6}>
                <Form.Item name="notes" label="Ghi chú">
                  <Input size="small" placeholder="Ghi chú" />
                </Form.Item>
              </Col>
            </Row>
            <Button size="small" icon={<PlusOutlined />} onClick={handleAddExec} loading={addExecMutation.isPending}>
              Thêm hồ sơ TH
            </Button>
          </Form>
        )}
        <Table<PppExecutionRecordDto>
          rowKey="id"
          columns={execColumns}
          dataSource={executionRecords}
          size="small"
          pagination={{ pageSize: 5 }}
          locale={{ emptyText: 'Chưa có hồ sơ thực hiện' }}
        />
      </Card>
    </div>
  );
}
