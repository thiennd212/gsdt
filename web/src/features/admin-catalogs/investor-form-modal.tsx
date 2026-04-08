import { Modal, Form, Input, Radio, Switch } from 'antd';
import type {
  InvestorDto,
  CreateInvestorRequest,
  UpdateInvestorRequest,
} from './catalog-types';

// Investor type options (Loại nhà đầu tư)
const INVESTOR_TYPE_OPTIONS = [
  { value: 'Doanh nghiệp', label: 'Doanh nghiệp' },
  { value: 'Cá nhân', label: 'Cá nhân' },
  { value: 'Tổ chức khác', label: 'Tổ chức khác' },
];

interface InvestorFormModalProps {
  open: boolean;
  editingItem: InvestorDto | null;
  saving: boolean;
  onSubmit: (
    values: CreateInvestorRequest | (UpdateInvestorRequest & { id: string }),
  ) => void;
  onCancel: () => void;
}

// Modal form for Investor — handles both create and edit modes
export function InvestorFormModal({
  open,
  editingItem,
  saving,
  onSubmit,
  onCancel,
}: InvestorFormModalProps) {
  const [form] = Form.useForm();
  const isEdit = Boolean(editingItem);

  function handleOpen(isOpen: boolean) {
    if (isOpen && editingItem) {
      form.setFieldsValue(editingItem);
    } else if (isOpen) {
      form.resetFields();
      // Default investor type for new records
      form.setFieldValue('investorType', 'Doanh nghiệp');
    }
  }

  async function handleOk() {
    const values = await form.validateFields();
    if (isEdit && editingItem) {
      onSubmit({ id: editingItem.id, ...values });
    } else {
      onSubmit(values);
    }
  }

  return (
    <Modal
      open={open}
      title={isEdit ? 'Chỉnh sửa nhà đầu tư' : 'Thêm mới nhà đầu tư'}
      onCancel={onCancel}
      onOk={handleOk}
      okText={isEdit ? 'Lưu thông tin' : 'Thêm mới'}
      cancelText="Quay lại"
      confirmLoading={saving}
      destroyOnHidden
      afterOpenChange={handleOpen}
      width={520}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="investorType"
          label="Loại nhà đầu tư"
          rules={[{ required: true, message: 'Vui lòng chọn loại nhà đầu tư' }]}
        >
          <Radio.Group options={INVESTOR_TYPE_OPTIONS} optionType="button" />
        </Form.Item>
        <Form.Item
          name="businessIdOrCccd"
          label="Mã số thuế / CCCD"
          rules={[
            { required: true, message: 'Vui lòng nhập mã số thuế hoặc CCCD' },
            { max: 20, message: 'Tối đa 20 ký tự' },
          ]}
        >
          <Input placeholder="Nhập mã số thuế hoặc số CCCD" />
        </Form.Item>
        <Form.Item
          name="nameVi"
          label="Tên nhà đầu tư (Tiếng Việt)"
          rules={[
            { required: true, message: 'Vui lòng nhập tên nhà đầu tư' },
            { max: 500, message: 'Tối đa 500 ký tự' },
          ]}
        >
          <Input placeholder="Nhập tên nhà đầu tư bằng tiếng Việt" />
        </Form.Item>
        <Form.Item
          name="nameEn"
          label="Tên nhà đầu tư (Tiếng Anh)"
          rules={[{ max: 500, message: 'Tối đa 500 ký tự' }]}
        >
          <Input placeholder="Enter investor name in English (optional)" />
        </Form.Item>
        {isEdit && (
          <Form.Item name="isActive" label="Trạng thái" valuePropName="checked">
            <Switch checkedChildren="Hoạt động" unCheckedChildren="Ngừng" />
          </Form.Item>
        )}
      </Form>
    </Modal>
  );
}
