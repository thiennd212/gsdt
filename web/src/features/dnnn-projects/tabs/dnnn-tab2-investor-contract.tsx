import { useEffect } from 'react';
import { Form, Select, Row, Col, Button, Input, Card, message } from 'antd';
import { DatePickerMaxToday, FileUploadField } from '@/features/shared/components';
import { useDynamicCatalog } from '@/features/domestic-projects/domestic-project-api';
import { useUpsertDnnnInvestorSelection, useDnnnProject } from '../dnnn-project-api';
import dayjs from 'dayjs';

interface Tab2Props {
  projectId: string;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
}

const SELECTION_FORM_OPTIONS = [
  { value: 1, label: 'Đấu thầu rộng rãi' },
  { value: 2, label: 'Đấu thầu hạn chế' },
  { value: 3, label: 'Chỉ định thầu' },
  { value: 4, label: 'Hình thức khác' },
];

// DnnnTab2InvestorContract — hình thức lựa chọn NĐT + thông tin NĐT cho dự án DNNN.
// Simpler than PPP Tab2: no TMĐT/contract sections, investor is free-text (not catalog).
export function DnnnTab2InvestorContract({ projectId, mode, onSaved }: Tab2Props) {
  const [form] = Form.useForm();
  const isReadonly = mode === 'detail';
  const { data: project } = useDnnnProject(projectId);
  const { data: investors = [] } = useDynamicCatalog('investors');

  const upsertMutation = useUpsertDnnnInvestorSelection();

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
        onSuccess: () => { message.success('Lưu thông tin NĐT thành công'); onSaved?.(); },
        onError: () => message.error('Lưu thất bại'),
      },
    );
  }

  return (
    <div>
      <Card size="small" title="Lựa chọn nhà đầu tư" style={{ marginBottom: 16 }}>
        <Form form={form} layout="vertical" disabled={isReadonly}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="selectionFormId" label="Hình thức lựa chọn NĐT">
                <Select placeholder="Chọn hình thức" options={SELECTION_FORM_OPTIONS} allowClear />
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="investorIds" label="Nhà đầu tư (danh mục)">
                <Select
                  mode="multiple"
                  placeholder="Chọn nhà đầu tư từ danh mục"
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
              <Form.Item name="decisionNumber" label="Số QĐ phê duyệt lựa chọn NĐT">
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
          {!isReadonly && (
            <Button type="primary" onClick={handleSave} loading={upsertMutation.isPending}>
              Lưu thông tin NĐT
            </Button>
          )}
        </Form>
      </Card>
    </div>
  );
}
