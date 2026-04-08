import { useEffect } from 'react';
import { Form, Select, Row, Col, Button, Input, message } from 'antd';
import { DatePickerMaxToday, FileUploadField } from '@/features/shared/components';
import { useSeedCatalog, useDynamicCatalog } from '@/features/domestic-projects/domestic-project-api';
import { useUpsertInvestorSelection, usePppProject } from '../ppp-project-api';
import dayjs from 'dayjs';

interface InvestorSelectionProps {
  projectId: string;
  disabled?: boolean;
}

const SELECTION_FORM_OPTIONS = [
  { value: 1, label: 'Đấu thầu rộng rãi' },
  { value: 2, label: 'Đấu thầu hạn chế' },
  { value: 3, label: 'Chỉ định thầu' },
  { value: 4, label: 'Hình thức khác' },
];

// PppTab2InvestorSelection — hình thức lựa chọn NĐT, danh sách NĐT, QĐ phê duyệt.
export function PppTab2InvestorSelection({ projectId, disabled }: InvestorSelectionProps) {
  const [form] = Form.useForm();
  const { data: project } = usePppProject(projectId);
  const { data: investors = [] } = useDynamicCatalog('investors');

  const upsertMutation = useUpsertInvestorSelection();

  // Pre-fill existing investor selection data
  useEffect(() => {
    const sel = project?.investorSelection;
    if (sel) {
      form.setFieldsValue({
        selectionFormId: sel.selectionFormId,
        investorIds: sel.investorIds,
        decisionNumber: sel.decisionNumber,
        decisionDate: sel.decisionDate ? dayjs(sel.decisionDate) : null,
        fileId: sel.fileId,
      });
    }
  }, [project, form]);

  async function handleSave() {
    const values = await form.validateFields();
    upsertMutation.mutate(
      {
        projectId,
        ...values,
        decisionDate: values.decisionDate?.format('YYYY-MM-DD') ?? null,
      },
      {
        onSuccess: () => message.success('Lưu thông tin NĐT thành công'),
        onError: () => message.error('Lưu thất bại'),
      },
    );
  }

  return (
    <Form form={form} layout="vertical" disabled={disabled}>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="selectionFormId" label="Hình thức lựa chọn NĐT">
            <Select placeholder="Chọn hình thức" options={SELECTION_FORM_OPTIONS} allowClear />
          </Form.Item>
        </Col>
        <Col span={16}>
          <Form.Item name="investorIds" label="Nhà đầu tư">
            <Select
              mode="multiple"
              placeholder="Chọn nhà đầu tư"
              showSearch
              filterOption={(input, opt) =>
                String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              options={(investors as { id: string; name: string; isActive: boolean }[])
                .filter((i) => i.isActive)
                .map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="decisionNumber" label="Số QĐ phê duyệt">
            <Input placeholder="Số quyết định" />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="decisionDate" label="Ngày QĐ">
            <DatePickerMaxToday placeholder="Chọn ngày" style={{ width: '100%' }} />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="fileId" label="Văn bản đính kèm">
            <FileUploadField accept=".pdf" maxCount={1} />
          </Form.Item>
        </Col>
      </Row>
      {!disabled && (
        <Button type="primary" onClick={handleSave} loading={upsertMutation.isPending}>
          Lưu thông tin NĐT
        </Button>
      )}
    </Form>
  );
}
