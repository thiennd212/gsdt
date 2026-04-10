import { Table, Button, Select, Input, Row, Col, Form, Popconfirm, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { MoneyInput, DatePickerMaxToday } from '@/features/shared/components';
import { useDynamicCatalog, useAddDecision, useDeleteDecision, useDomesticProject } from '../domestic-project-api';
import type { DecisionDto } from '../domestic-project-types';

interface DecisionsZoneProps {
  /** Null in create mode — form shown but disabled */
  projectId: string | null;
  disabled?: boolean;
}

// Decision type options per SRS mockup
const DECISION_TYPES = [
  { value: 0, label: 'Quyết định đầu tư (Ban đầu)' },
  { value: 1, label: 'Quyết định điều chỉnh' },
  { value: 2, label: 'Quyết định gia hạn' },
];

// Reusable zone title — purple left border (same pattern as other zones in tab1-general-info)
const zoneTitle = (text: string) => (
  <div style={{ borderLeft: '4px solid #5B21B6', paddingLeft: 12, marginBottom: 16 }}>
    <h3 style={{ margin: 0, fontWeight: 700, textTransform: 'uppercase', color: '#1e293b', fontSize: 13, letterSpacing: '0.04em' }}>
      {text}
    </h3>
  </div>
);

// Zone 5: Quyết định Đầu tư — redesigned per SRS mockup.
// Layout:
//   Title: purple left border "QUYẾT ĐỊNH ĐẦU TƯ"
//   Row 1: Loại quyết định | Số quyết định | Ngày quyết định | CQ quyết định đầu tư  (4×span6)
//   Sub-section "CHI TIẾT NGUỒN VỐN THEO QUYẾT ĐỊNH":
//     Row 1: Tổng mức đầu tư | Nguồn vốn đầu tư | Vốn đầu tư công  (3×span8, triệu VND suffix)
//     Row 2: Vốn khác | Ngân sách trung ương | Ngân sách địa phương  (3×span8)
//     Row 3: Vốn đầu tư khác  (1 col)
//   Decisions table listing existing records.
// When projectId is null (create mode): form and table shown but disabled with placeholder.
export function Tab1DecisionsZone({ projectId, disabled }: DecisionsZoneProps) {
  const [form] = Form.useForm();

  // Only fetch project data when projectId is available
  const { data: project } = useDomesticProject(projectId ?? undefined);
  const decisions = project?.decisions ?? [];

  // Investment decision authorities dynamic catalog
  const { data: decisionAuthorities = [] } = useDynamicCatalog('investment-decision-authorities');

  const addMutation = useAddDecision();
  const deleteMutation = useDeleteDecision();

  // Form is interactive only when projectId exists and not in readonly mode
  const isFormDisabled = disabled || !projectId;

  async function handleAdd() {
    if (!projectId) return;
    let values;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }
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
    if (!projectId) return;
    deleteMutation.mutate({ projectId, decisionId }, {
      onSuccess: () => message.success('Đã xóa'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const columns: ColumnsType<DecisionDto> = [
    { title: 'Loại', dataIndex: 'decisionType', key: 'type', width: 180, render: (v: number) => DECISION_TYPES.find((d) => d.value === v)?.label ?? v },
    { title: 'Số QĐ', dataIndex: 'decisionNumber', key: 'number', width: 120 },
    { title: 'Ngày', dataIndex: 'decisionDate', key: 'date', width: 110, render: (v: string) => dayjs(v).format('DD/MM/YYYY') },
    { title: 'Cơ quan QĐ ĐT', dataIndex: 'decisionAuthority', key: 'auth', width: 200, ellipsis: true },
    { title: 'TMĐT (triệu)', dataIndex: 'totalInvestment', key: 'total', width: 130, render: (v: number) => v?.toLocaleString('vi-VN') },
    {
      title: '', key: 'actions', width: 60,
      render: (_, r) => !disabled && projectId && (
        <Popconfirm title="Xóa quyết định này?" onConfirm={() => handleDelete(r.id)} okText="Xóa" cancelText="Hủy">
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  return (
    <div style={{ background: '#fff', border: '1px solid #f0f0f0', borderRadius: 8, padding: '20px 24px', marginBottom: 16 }}>
      {zoneTitle('Quyết định đầu tư')}

      {/* Create mode placeholder */}
      {!projectId && (
        <div style={{ color: '#94a3b8', fontSize: 13, marginBottom: 16, fontStyle: 'italic' }}>
          Lưu thông tin dự án trước để thêm quyết định đầu tư.
        </div>
      )}

      {/* Input form — shown always, disabled when no projectId or readonly */}
      {!disabled && (
        <Form form={form} layout="vertical" disabled={isFormDisabled} style={{ marginBottom: 16 }}>
          {/* Row 1: Loại QĐ | Số QĐ | Ngày QĐ | CQ QĐ ĐT */}
          <Row gutter={16}>
            <Col span={6}>
              <Form.Item name="decisionType" label={<span>Loại quyết định <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn loại quyết định' }]}>
                <Select placeholder="Chọn loại" options={DECISION_TYPES} />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="decisionNumber" label={<span>Số quyết định <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng nhập số quyết định' }]}>
                <Input placeholder="VD: 123/QĐ-TTg" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="decisionDate" label={<span>Ngày quyết định <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn ngày' }]}>
                <DatePickerMaxToday placeholder="dd/mm/yyyy" style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="decisionAuthority" label={<span>CQ quyết định đầu tư <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn cơ quan' }]}>
                <Select
                  placeholder="Chọn cơ quan"
                  showSearch
                  filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
                  options={decisionAuthorities
                    .filter((i: { isActive?: boolean }) => i.isActive !== false)
                    .map((i: { id: string; name: string }) => ({ value: i.name, label: i.name }))}
                />
              </Form.Item>
            </Col>
          </Row>

          {/* Sub-section: Chi tiết nguồn vốn theo quyết định.
              Field names match DecisionDto (BE contract): totalInvestment, otherPublicCapital,
              otherCapital, centralBudget, localBudget. */}
          <div style={{ border: '1px solid #e2e8f0', borderRadius: 6, padding: '16px 16px 4px', marginBottom: 16 }}>
            <div style={{ fontSize: 12, fontWeight: 600, color: '#475569', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: 12 }}>
              Chi tiết nguồn vốn theo quyết định
            </div>

            {/* Sub-row 1: Tổng mức ĐT(*) | Nguồn vốn ĐT công | Vốn khác */}
            <Row gutter={16}>
              <Col span={8}>
                <Form.Item name="totalInvestment" label={<span>Tổng mức đầu tư <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng nhập tổng mức đầu tư' }]}>
                  <MoneyInput unit="triệu VND" />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item name="otherPublicCapital" label="Nguồn vốn đầu tư công">
                  <MoneyInput unit="triệu VND" />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item name="otherCapital" label="Vốn khác">
                  <MoneyInput unit="triệu VND" />
                </Form.Item>
              </Col>
            </Row>

            {/* Sub-row 2: Ngân sách TW | Ngân sách ĐP */}
            <Row gutter={16}>
              <Col span={8}>
                <Form.Item name="centralBudget" label="Ngân sách trung ương">
                  <MoneyInput unit="triệu VND" />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item name="localBudget" label="Ngân sách địa phương">
                  <MoneyInput unit="triệu VND" />
                </Form.Item>
              </Col>
            </Row>
          </div>

          {/* Add button */}
          <div style={{ textAlign: 'right', marginBottom: 8 }}>
            <Button
              icon={<PlusOutlined />}
              type="primary"
              onClick={handleAdd}
              loading={addMutation.isPending}
              disabled={isFormDisabled}
            >
              Thêm quyết định
            </Button>
          </div>
        </Form>
      )}

      {/* Decisions table */}
      <Table<DecisionDto>
        rowKey="id"
        columns={columns}
        dataSource={decisions}
        size="small"
        locale={{ emptyText: projectId ? 'Chưa có quyết định đầu tư' : 'Lưu dự án trước để xem danh sách quyết định' }}
        pagination={{ pageSize: 5, showTotal: (t) => `${t} quyết định` }}
      />
    </div>
  );
}
