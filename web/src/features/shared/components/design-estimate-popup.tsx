import { useEffect, useMemo } from 'react';
import { Modal, Form, Input, Row, Col, Divider, Button, message } from 'antd';
import dayjs from 'dayjs';
import { DatePickerMaxToday, FileUploadField } from '@/features/shared/components';
import { DesignEstimateCostFields } from './design-estimate-cost-fields';
import { DesignEstimateItemsTable } from './design-estimate-items-table';
import type { DesignEstimateDto, DesignEstimateItemDto } from '@/features/ppp-projects/ppp-project-types';

interface DesignEstimatePopupProps {
  projectId: string;
  open: boolean;
  onClose: () => void;
  editingEstimate?: DesignEstimateDto | null;
  onSaved?: () => void;
  /** Injected API hooks — keeps popup decoupled from concrete API module */
  apiHooks: {
    useAdd: () => { mutate: (vars: { projectId: string } & Record<string, unknown>, opts: { onSuccess: () => void; onError: () => void }) => void; isPending: boolean };
    useUpdate: () => { mutate: (vars: { projectId: string; estimateId: string } & Record<string, unknown>, opts: { onSuccess: () => void; onError: () => void }) => void; isPending: boolean };
  };
}

type FormValues = {
  decisionNumber: string;
  decisionDate: dayjs.Dayjs | null;
  decisionAuthority: string;
  signer: string;
  summary: string;
  fileId: string;
  costEquipment: number;
  costConstruction: number;
  costLandCompensation: number;
  costManagement: number;
  costConsultancy: number;
  costContingency: number;
  costOther: number;
  items: Omit<DesignEstimateItemDto, 'id'>[];
};

// DesignEstimatePopup — modal form for adding/editing a QĐ phê duyệt TKKT (design estimate).
// Cost totals are auto-summed for UX preview; totalEstimate is NOT submitted to API.
export function DesignEstimatePopup({ projectId, open, onClose, editingEstimate, onSaved, apiHooks }: DesignEstimatePopupProps) {
  const [form] = Form.useForm<FormValues>();
  const addHook = apiHooks.useAdd();
  const updateHook = apiHooks.useUpdate();
  const saving = addHook.isPending || updateHook.isPending;
  const isEdit = Boolean(editingEstimate);

  useEffect(() => {
    if (open && editingEstimate) {
      form.setFieldsValue({
        ...editingEstimate,
        decisionDate: editingEstimate.decisionDate ? dayjs(editingEstimate.decisionDate) : null,
        items: editingEstimate.items,
      });
    } else if (open) {
      form.resetFields();
    }
  }, [open, editingEstimate, form]);

  // Watch 7 cost fields for auto-sum display
  const eq = Form.useWatch('costEquipment', form) ?? 0;
  const cn = Form.useWatch('costConstruction', form) ?? 0;
  const lc = Form.useWatch('costLandCompensation', form) ?? 0;
  const mg = Form.useWatch('costManagement', form) ?? 0;
  const cs = Form.useWatch('costConsultancy', form) ?? 0;
  const ct = Form.useWatch('costContingency', form) ?? 0;
  const ot = Form.useWatch('costOther', form) ?? 0;
  const totalEstimate = useMemo(() => eq + cn + lc + mg + cs + ct + ot, [eq, cn, lc, mg, cs, ct, ot]);

  async function handleOk() {
    const values = await form.validateFields();
    const payload = { projectId, ...values, decisionDate: values.decisionDate?.format('YYYY-MM-DD') ?? null };
    const opts = {
      onSuccess: () => {
        message.success(isEdit ? 'Cập nhật TKKT thành công' : 'Thêm TKKT thành công');
        form.resetFields();
        onSaved?.();
        onClose();
      },
      onError: () => message.error('Thao tác thất bại, vui lòng thử lại'),
    };
    if (isEdit && editingEstimate) {
      updateHook.mutate({ ...payload, estimateId: editingEstimate.id }, opts);
    } else {
      addHook.mutate(payload, opts);
    }
  }

  return (
    <Modal
      title={isEdit ? 'Chỉnh sửa QĐ phê duyệt TKKT' : 'Thêm QĐ phê duyệt TKKT'}
      open={open}
      onCancel={onClose}
      width={860}
      footer={[
        <Button key="cancel" onClick={onClose}>Hủy</Button>,
        <Button key="ok" type="primary" loading={saving} onClick={handleOk}>
          {isEdit ? 'Cập nhật' : 'Thêm mới'}
        </Button>,
      ]}
      destroyOnClose
    >
      <Form form={form} layout="vertical">
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="decisionNumber" label="Số QĐ phê duyệt" rules={[{ required: true, message: 'Vui lòng nhập số QĐ' }]}>
              <Input placeholder="Số quyết định" />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="decisionDate" label="Ngày QĐ">
              <DatePickerMaxToday placeholder="Chọn ngày" style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="decisionAuthority" label="Cơ quan phê duyệt" rules={[{ required: true, message: 'Vui lòng nhập cơ quan' }]}>
              <Input placeholder="Cơ quan phê duyệt" />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="signer" label="Người ký">
              <Input placeholder="Họ tên người ký" />
            </Form.Item>
          </Col>
          <Col span={16}>
            <Form.Item name="summary" label="Tóm tắt nội dung">
              <Input.TextArea rows={2} placeholder="Tóm tắt nội dung QĐ" />
            </Form.Item>
          </Col>
        </Row>
        <Form.Item name="fileId" label="Văn bản đính kèm">
          <FileUploadField accept=".pdf" maxCount={1} />
        </Form.Item>

        <Divider orientation="left" plain>Chi phí TKTT-DA (triệu VNĐ)</Divider>
        {/* Cost fields extracted to keep this file under 200 LOC */}
        <DesignEstimateCostFields totalEstimate={totalEstimate} />

        <Divider orientation="left" plain>Danh mục hạng mục</Divider>
        <Form.Item name="items" noStyle>
          <DesignEstimateItemsTable
            items={form.getFieldValue('items') ?? []}
            onChange={(items) => form.setFieldValue('items', items)}
          />
        </Form.Item>
      </Form>
    </Modal>
  );
}
