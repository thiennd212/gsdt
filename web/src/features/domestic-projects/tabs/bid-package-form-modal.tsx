import { Modal, Form, Input, Select, Checkbox, Row, Col, InputNumber } from 'antd';
import { MoneyInput } from '@/features/shared/components';
import { useSeedCatalog, useDynamicCatalog } from '../domestic-project-api';
import { useContractorSelectionPlans } from '@/features/admin-catalogs/catalog-api';

interface BidPackageFormModalProps {
  open: boolean;
  onCancel: () => void;
  onSubmit: (values: Record<string, unknown>) => void;
  saving: boolean;
}

const DURATION_UNITS = [
  { value: 0, label: 'Ngày' },
  { value: 1, label: 'Tháng' },
  { value: 2, label: 'Năm' },
];

// Popup modal for adding a bid package (~15 fields).
// Includes KHLCNT combobox (v1.1), checkboxes, catalog selects, and money inputs.
export function BidPackageFormModal({ open, onCancel, onSubmit, saving }: BidPackageFormModalProps) {
  const [form] = Form.useForm();

  const { data: bidSelectionForms = [] } = useSeedCatalog('bid-selection-forms');
  const { data: bidSelectionMethods = [] } = useSeedCatalog('bid-selection-methods');
  const { data: contractForms = [] } = useSeedCatalog('contract-forms');
  const { data: bidSectorTypes = [] } = useSeedCatalog('bid-sector-types');
  const { data: contractors = [] } = useDynamicCatalog('contractors');
  const { data: khlcntPlans = [] } = useContractorSelectionPlans();

  function handleOpen(isOpen: boolean) {
    if (isOpen) form.resetFields();
  }

  async function handleOk() {
    const values = await form.validateFields();
    onSubmit(values);
  }

  return (
    <Modal
      open={open}
      title="Thêm gói thầu"
      onCancel={onCancel}
      onOk={handleOk}
      okText="Lưu thông tin"
      cancelText="Quay lại"
      confirmLoading={saving}
      destroyOnHidden
      afterOpenChange={handleOpen}
      width={800}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Row gutter={16}>
          <Col span={16}>
            <Form.Item name="name" label="Tên gói thầu" rules={[{ required: true, message: 'Vui lòng nhập tên gói thầu' }]}>
              <Input placeholder="Nhập tên gói thầu" />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="contractorSelectionPlanId" label="KHLCNT (v1.1)">
              <Select
                placeholder="Chọn KHLCNT"
                allowClear showSearch
                filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
                options={khlcntPlans.filter((p) => p.isActive).map((p) => ({ value: p.id, label: p.nameVi }))}
              />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={6}>
            <Form.Item name="bidSelectionFormId" label="Hình thức LCNT" rules={[{ required: true }]}>
              <Select placeholder="Chọn" options={bidSelectionForms.map((i) => ({ value: i.id, label: i.name }))} />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="bidSelectionMethodId" label="Phương thức LCNT" rules={[{ required: true }]}>
              <Select placeholder="Chọn" options={bidSelectionMethods.map((i) => ({ value: i.id, label: i.name }))} />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="contractFormId" label="Hình thức HĐ" rules={[{ required: true }]}>
              <Select placeholder="Chọn" options={contractForms.map((i) => ({ value: i.id, label: i.name }))} />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="bidSectorTypeId" label="Lĩnh vực LCNT" rules={[{ required: true }]}>
              <Select placeholder="Chọn" options={bidSectorTypes.map((i) => ({ value: i.id, label: i.name }))} />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={6}>
            <Form.Item name="duration" label="Thời gian">
              <InputNumber min={0} style={{ width: '100%' }} placeholder="Số" />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="durationUnit" label="Đơn vị TG">
              <Select placeholder="Chọn" options={DURATION_UNITS} />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="estimatedPrice" label="Giá dự toán">
              <MoneyInput />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="winningPrice" label="Giá trúng thầu">
              <MoneyInput />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="winningContractorId" label="ĐV trúng thầu">
              <Select
                placeholder="Chọn nhà thầu"
                allowClear showSearch
                filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
                options={contractors.filter((c) => c.isActive).map((c) => ({ value: c.id, label: c.name }))}
              />
            </Form.Item>
          </Col>
          <Col span={4}>
            <Form.Item name="resultDecisionNumber" label="Số QĐ kết quả">
              <Input placeholder="Số QĐ" />
            </Form.Item>
          </Col>
          <Col span={4}>
            <Form.Item name="isDesignReview" valuePropName="checked" label=" ">
              <Checkbox>Thiết kế</Checkbox>
            </Form.Item>
          </Col>
          <Col span={4}>
            <Form.Item name="isSupervision" valuePropName="checked" label=" ">
              <Checkbox>Giám sát</Checkbox>
            </Form.Item>
          </Col>
        </Row>
        <Form.Item name="notes" label="Ghi chú">
          <Input.TextArea rows={2} placeholder="Ghi chú" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
