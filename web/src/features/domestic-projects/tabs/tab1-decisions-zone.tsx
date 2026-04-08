import { useState } from 'react';
import { Table, Button, Space, Select, Input, Row, Col, Form, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { MoneyInput, DatePickerMaxToday } from '@/features/shared/components';
import { useSeedCatalog, useAddDecision, useDeleteDecision, useDomesticProject } from '../domestic-project-api';
import type { DecisionDto } from '../domestic-project-types';

interface DecisionsZoneProps {
  projectId: string;
  disabled?: boolean;
}

const DECISION_TYPES = [
  { value: 0, label: 'QĐ đầu tư ban đầu' },
  { value: 1, label: 'QĐ điều chỉnh' },
];

// Zone 5: QĐ Đầu tư — inline form + table for investment decisions
export function Tab1DecisionsZone({ projectId, disabled }: DecisionsZoneProps) {
  const [form] = Form.useForm();
  const { data: project } = useDomesticProject(projectId);
  const decisions = project?.decisions ?? [];

  const { data: adjustmentContents = [] } = useSeedCatalog('adjustment-contents');
  const addMutation = useAddDecision();
  const deleteMutation = useDeleteDecision();

  const decisionType = Form.useWatch('decisionType', form);

  async function handleAdd() {
    const values = await form.validateFields();
    addMutation.mutate(
      {
        projectId,
        ...values,
        decisionDate: values.decisionDate?.format('YYYY-MM-DD'),
      },
      {
        onSuccess: () => { message.success('Thêm QĐ thành công'); form.resetFields(); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  function handleDelete(decisionId: string) {
    deleteMutation.mutate({ projectId, decisionId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<DecisionDto> = [
    { title: 'Loại', dataIndex: 'decisionType', key: 'type', width: 140, render: (v: number) => DECISION_TYPES.find((d) => d.value === v)?.label ?? v },
    { title: 'Số QĐ', dataIndex: 'decisionNumber', key: 'number', width: 120 },
    { title: 'Ngày', dataIndex: 'decisionDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Cơ quan', dataIndex: 'decisionAuthority', key: 'auth', width: 150, ellipsis: true },
    { title: 'TMĐT', dataIndex: 'totalInvestment', key: 'total', width: 120, render: (v: number) => v?.toLocaleString('vi-VN') },
    {
      title: '', key: 'actions', width: 60,
      render: (_, r) => !disabled && (
        <Popconfirm title="Xóa QĐ?" onConfirm={() => handleDelete(r.id)} okText="Xóa" cancelText="Hủy">
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  return (
    <div>
      {!disabled && (
        <Form form={form} layout="vertical" style={{ marginBottom: 16 }}>
          <Row gutter={12}>
            <Col span={4}>
              <Form.Item name="decisionType" label="Loại QĐ" rules={[{ required: true }]}>
                <Select placeholder="Loại" options={DECISION_TYPES} />
              </Form.Item>
            </Col>
            {decisionType === 1 && (
              <Col span={5}>
                <Form.Item name="adjustmentContentId" label="Nội dung điều chỉnh">
                  <Select placeholder="Chọn" options={adjustmentContents.map((i) => ({ value: i.id, label: i.name }))} />
                </Form.Item>
              </Col>
            )}
            <Col span={4}>
              <Form.Item name="decisionNumber" label="Số QĐ" rules={[{ required: true }]}>
                <Input placeholder="Số QĐ" />
              </Form.Item>
            </Col>
            <Col span={4}>
              <Form.Item name="decisionDate" label="Ngày QĐ" rules={[{ required: true }]}>
                <DatePickerMaxToday placeholder="Ngày" />
              </Form.Item>
            </Col>
            <Col span={5}>
              <Form.Item name="decisionAuthority" label="Cơ quan QĐ ĐT" rules={[{ required: true }]}>
                <Input placeholder="Cơ quan" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={12}>
            <Col span={4}><Form.Item name="totalInvestment" label="TMĐT"><MoneyInput /></Form.Item></Col>
            <Col span={4}><Form.Item name="centralBudget" label="NSTW"><MoneyInput /></Form.Item></Col>
            <Col span={4}><Form.Item name="localBudget" label="NSĐP"><MoneyInput /></Form.Item></Col>
            <Col span={4}><Form.Item name="otherPublicCapital" label="Vốn ĐTC khác"><MoneyInput /></Form.Item></Col>
            <Col span={4}><Form.Item name="otherCapital" label="Vốn khác"><MoneyInput /></Form.Item></Col>
            <Col span={4} style={{ display: 'flex', alignItems: 'end', paddingBottom: 24 }}>
              <Button icon={<PlusOutlined />} type="primary" onClick={handleAdd} loading={addMutation.isPending}>
                Lưu thông tin
              </Button>
            </Col>
          </Row>
        </Form>
      )}
      <Table<DecisionDto>
        rowKey="id"
        columns={columns}
        dataSource={decisions}
        size="small"
        pagination={{ pageSize: 5, showTotal: (t) => `${t} quyết định` }}
      />
    </div>
  );
}
