import { useState } from 'react';
import { Table, Button, Space, Popconfirm, Form, Input, Row, Col, InputNumber, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { DatePickerMaxToday } from '@/features/shared/components';
import { useAddNdtDecision, useDeleteNdtDecision, useNdtProject } from '../ndt-project-api';
import type { NdtInvestmentDecisionDto } from '../ndt-project-types';

interface DecisionsZoneProps {
  projectId: string;
  disabled?: boolean;
}

const DECISION_TYPE_CTDT = 0;
const DECISION_TYPE_DT = 1;

// NdtTab1DecisionsZone — two sub-sections: QĐ CTĐT and QĐ ĐT.
// NĐT capital split: equityCapital (Vốn CSH) + odaLoanCapital (Vốn ODA) + creditLoanCapital (Vốn TCTD) = totalInvestment.
function NdtDecisionForm({ projectId, decisionType }: { projectId: string; decisionType: number }) {
  const [form] = Form.useForm();
  const addMutation = useAddNdtDecision();

  // Auto-sum total from three capital fields
  function handleCapitalChange() {
    const eq = form.getFieldValue('equityCapital') ?? 0;
    const oda = form.getFieldValue('odaLoanCapital') ?? 0;
    const credit = form.getFieldValue('creditLoanCapital') ?? 0;
    form.setFieldValue('totalInvestment', eq + oda + credit);
  }

  async function handleAdd() {
    const values = await form.validateFields();
    addMutation.mutate(
      {
        projectId,
        decisionType,
        ...values,
        decisionDate: values.decisionDate?.format('YYYY-MM-DD'),
      },
      {
        onSuccess: () => { message.success('Thêm quyết định thành công'); form.resetFields(); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  return (
    <Form form={form} layout="vertical" style={{ marginBottom: 12 }}>
      <Row gutter={12}>
        <Col span={4}>
          <Form.Item name="decisionNumber" label="Số QĐ" rules={[{ required: true, message: 'Bắt buộc' }]}>
            <Input size="small" placeholder="Số QĐ" />
          </Form.Item>
        </Col>
        <Col span={4}>
          <Form.Item name="decisionDate" label="Ngày QĐ">
            <DatePickerMaxToday size="small" style={{ width: '100%' }} />
          </Form.Item>
        </Col>
        <Col span={5}>
          <Form.Item name="decisionAuthority" label="Cơ quan ban hành" rules={[{ required: true, message: 'Bắt buộc' }]}>
            <Input size="small" placeholder="Cơ quan" />
          </Form.Item>
        </Col>
        <Col span={3}>
          <Form.Item name="equityCapital" label="Vốn CSH (tr.đ)">
            <InputNumber size="small" min={0} style={{ width: '100%' }} onChange={handleCapitalChange} />
          </Form.Item>
        </Col>
        <Col span={3}>
          <Form.Item name="odaLoanCapital" label="Vốn ODA (tr.đ)">
            <InputNumber size="small" min={0} style={{ width: '100%' }} onChange={handleCapitalChange} />
          </Form.Item>
        </Col>
        <Col span={3}>
          <Form.Item name="creditLoanCapital" label="Vốn TCTD (tr.đ)">
            <InputNumber size="small" min={0} style={{ width: '100%' }} onChange={handleCapitalChange} />
          </Form.Item>
        </Col>
        <Col span={2}>
          <Form.Item name="totalInvestment" label="Tổng (tr.đ)">
            <InputNumber size="small" min={0} style={{ width: '100%' }} disabled />
          </Form.Item>
        </Col>
      </Row>
      <Button size="small" icon={<PlusOutlined />} onClick={handleAdd} loading={addMutation.isPending}>
        Thêm QĐ
      </Button>
    </Form>
  );
}

export function NdtTab1DecisionsZone({ projectId, disabled }: DecisionsZoneProps) {
  const { data: project } = useNdtProject(projectId);
  const decisions = project?.decisions ?? [];
  const deleteMutation = useDeleteNdtDecision();

  const ctdtDecisions = decisions.filter((d) => d.decisionType === DECISION_TYPE_CTDT);
  const dtDecisions = decisions.filter((d) => d.decisionType === DECISION_TYPE_DT);

  function handleDelete(decisionId: string) {
    deleteMutation.mutate({ projectId, decisionId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<NdtInvestmentDecisionDto> = [
    { title: 'Số QĐ', dataIndex: 'decisionNumber', key: 'number', width: 120 },
    { title: 'Ngày', dataIndex: 'decisionDate', key: 'date', width: 100,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Cơ quan', dataIndex: 'decisionAuthority', key: 'authority', ellipsis: true },
    { title: 'Vốn CSH', dataIndex: 'equityCapital', key: 'eq', width: 120,
      render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'Vốn ODA', dataIndex: 'odaLoanCapital', key: 'oda', width: 120,
      render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'Vốn TCTD', dataIndex: 'creditLoanCapital', key: 'credit', width: 120,
      render: (v: number) => v?.toLocaleString('vi-VN') },
    { title: 'Tổng TMĐT', dataIndex: 'totalInvestment', key: 'total', width: 130,
      render: (v: number) => v?.toLocaleString('vi-VN') },
    {
      title: '', key: 'actions', width: 50,
      render: (_, r) => !disabled && (
        <Popconfirm title="Xóa quyết định?" onConfirm={() => handleDelete(r.id)} okText="Xóa" cancelText="Hủy">
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  return (
    <Space direction="vertical" style={{ width: '100%' }} size={16}>
      {/* QĐ Chủ trương đầu tư */}
      <div>
        <div style={{ fontWeight: 600, marginBottom: 8 }}>QĐ Chủ trương đầu tư (CTĐT)</div>
        {!disabled && <NdtDecisionForm projectId={projectId} decisionType={DECISION_TYPE_CTDT} />}
        <Table<NdtInvestmentDecisionDto>
          rowKey="id" columns={columns} dataSource={ctdtDecisions}
          size="small" pagination={false} locale={{ emptyText: 'Chưa có QĐ CTĐT' }}
        />
      </div>

      {/* QĐ Đầu tư */}
      <div>
        <div style={{ fontWeight: 600, marginBottom: 8 }}>QĐ Đầu tư (ĐT)</div>
        {!disabled && <NdtDecisionForm projectId={projectId} decisionType={DECISION_TYPE_DT} />}
        <Table<NdtInvestmentDecisionDto>
          rowKey="id" columns={columns} dataSource={dtDecisions}
          size="small" pagination={false} locale={{ emptyText: 'Chưa có QĐ ĐT' }}
        />
      </div>
    </Space>
  );
}
