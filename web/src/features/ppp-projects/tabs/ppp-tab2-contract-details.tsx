import { useEffect } from 'react';
import { Form, Input, Select, Row, Col, Card, Button, Space, Divider, message } from 'antd';
import { DatePickerMaxToday } from '@/features/shared/components';
import { useUpsertContractInfo, usePppProject } from '../ppp-project-api';
import { PppTab2InvestorSelection } from './ppp-tab2-investor-selection';
import { PppTab2TmdtBreakdown } from './ppp-tab2-tmdt-breakdown';
import dayjs from 'dayjs';

interface Tab2Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

const DURATION_UNIT_OPTIONS = [
  { value: 1, label: 'Tháng' },
  { value: 2, label: 'Năm' },
];

// PppTab2ContractDetails — 4 sections: NĐT selection, TMĐT breakdown, tiến độ, HĐ ký kết.
export function PppTab2ContractDetails({ projectId, mode, onSaved }: Tab2Props) {
  const [form] = Form.useForm();
  const isReadonly = mode === 'detail';
  const { data: project } = usePppProject(projectId);
  const upsertMutation = useUpsertContractInfo();

  // Pre-fill contract info
  useEffect(() => {
    const ci = project?.contractInfo;
    if (ci) {
      form.setFieldsValue({
        ...ci,
        contractDate: ci.contractDate ? dayjs(ci.contractDate) : null,
        startDate: ci.startDate ? dayjs(ci.startDate) : null,
        endDate: ci.endDate ? dayjs(ci.endDate) : null,
      });
    }
  }, [project, form]);

  async function handleSave() {
    const values = await form.validateFields();
    upsertMutation.mutate(
      {
        projectId,
        ...values,
        contractDate: values.contractDate?.format('YYYY-MM-DD') ?? null,
        startDate: values.startDate?.format('YYYY-MM-DD') ?? null,
        endDate: values.endDate?.format('YYYY-MM-DD') ?? null,
      },
      {
        onSuccess: () => { message.success('Lưu thông tin HĐ thành công'); onSaved?.(); },
        onError: () => message.error('Lưu thất bại'),
      },
    );
  }

  return (
    <div>
      {/* Section 1: Lựa chọn nhà đầu tư */}
      <Card size="small" title="Lựa chọn nhà đầu tư (NĐT)" style={{ marginBottom: 16 }}>
        <PppTab2InvestorSelection projectId={projectId} disabled={isReadonly} />
      </Card>

      {/* Sections 2-4 share one Form instance so TmdtBreakdown can useFormInstance() */}
      <Form form={form} layout="vertical" disabled={isReadonly}>
        {/* Section 2: TMĐT breakdown */}
        <Card size="small" title="Cơ cấu Tổng mức đầu tư (TMĐT)" style={{ marginBottom: 16 }}>
          <PppTab2TmdtBreakdown disabled={isReadonly} />
        </Card>

        {/* Section 3: Tiến độ */}
        <Card size="small" title="Tiến độ thực hiện" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={6}>
              <Form.Item name="duration" label="Thời hạn HĐ">
                <Input type="number" placeholder="Thời hạn" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="durationUnit" label="Đơn vị">
                <Select placeholder="Đơn vị" options={DURATION_UNIT_OPTIONS} allowClear />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="startDate" label="Ngày bắt đầu">
                <DatePickerMaxToday placeholder="Ngày bắt đầu" style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="endDate" label="Ngày kết thúc">
                <DatePickerMaxToday placeholder="Ngày kết thúc" style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={24}>
              <Form.Item name="notes" label="Ghi chú tiến độ">
                <Input.TextArea rows={2} placeholder="Ghi chú" />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Section 4: HĐ ký kết */}
        <Card size="small" title="Hợp đồng ký kết" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="contractNumber" label="Số hợp đồng">
                <Input placeholder="Số HĐ" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="contractDate" label="Ngày ký HĐ">
                <DatePickerMaxToday placeholder="Ngày ký" style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {!isReadonly && (
          <Space>
            <Button type="primary" onClick={handleSave} loading={upsertMutation.isPending}>
              Lưu thông tin HĐ
            </Button>
          </Space>
        )}
      </Form>
    </div>
  );
}
