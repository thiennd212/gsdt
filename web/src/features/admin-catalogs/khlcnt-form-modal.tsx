import { Modal, Form, Input, DatePicker, Switch } from 'antd';
import dayjs from 'dayjs';
import type {
  ContractorSelectionPlanDto,
  CreateContractorSelectionPlanRequest,
  UpdateContractorSelectionPlanRequest,
} from './catalog-types';

interface KhlcntFormModalProps {
  open: boolean;
  editingItem: ContractorSelectionPlanDto | null;
  saving: boolean;
  onSubmit: (
    values:
      | CreateContractorSelectionPlanRequest
      | (UpdateContractorSelectionPlanRequest & { id: string }),
  ) => void;
  onCancel: () => void;
}

// Modal form for KHLCNT — 4 editable fields (NameVi, NameEn, SignedDate, SignedBy) + IsActive on edit
export function KhlcntFormModal({
  open,
  editingItem,
  saving,
  onSubmit,
  onCancel,
}: KhlcntFormModalProps) {
  const [form] = Form.useForm();
  const isEdit = Boolean(editingItem);

  function handleOpen(isOpen: boolean) {
    if (isOpen && editingItem) {
      form.setFieldsValue({
        ...editingItem,
        signedDate: editingItem.signedDate ? dayjs(editingItem.signedDate) : null,
      });
    } else if (isOpen) {
      form.resetFields();
    }
  }

  async function handleOk() {
    const values = await form.validateFields();
    const payload = {
      ...values,
      signedDate: values.signedDate?.format('YYYY-MM-DD'),
    };
    if (isEdit && editingItem) {
      onSubmit({ id: editingItem.id, ...payload });
    } else {
      onSubmit(payload);
    }
  }

  return (
    <Modal
      open={open}
      title={isEdit ? 'Chỉnh sửa KHLCNT' : 'Thêm mới KHLCNT'}
      onCancel={onCancel}
      onOk={handleOk}
      okText={isEdit ? 'Lưu thông tin' : 'Thêm mới'}
      cancelText="Quay lại"
      confirmLoading={saving}
      destroyOnHidden
      afterOpenChange={handleOpen}
      width={560}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="nameVi"
          label="Tên kế hoạch LCNT (Tiếng Việt)"
          rules={[
            { required: true, message: 'Vui lòng nhập tên kế hoạch (VN)' },
            { max: 500, message: 'Tối đa 500 ký tự' },
          ]}
        >
          <Input placeholder="Nhập tên kế hoạch lựa chọn nhà thầu" />
        </Form.Item>
        <Form.Item
          name="nameEn"
          label="Tên kế hoạch LCNT (Tiếng Anh)"
          rules={[
            { required: true, message: 'Vui lòng nhập tên kế hoạch (EN)' },
            { max: 500, message: 'Tối đa 500 ký tự' },
          ]}
        >
          <Input placeholder="Enter contractor selection plan name" />
        </Form.Item>
        <Form.Item
          name="signedDate"
          label="Ngày ký"
          rules={[{ required: true, message: 'Vui lòng chọn ngày ký' }]}
        >
          <DatePicker
            style={{ width: '100%' }}
            format="DD/MM/YYYY"
            disabledDate={(current) => current && current > dayjs().endOf('day')}
            placeholder="Chọn ngày ký"
          />
        </Form.Item>
        <Form.Item
          name="signedBy"
          label="Người ký"
          rules={[
            { required: true, message: 'Vui lòng nhập người ký' },
            { max: 200, message: 'Tối đa 200 ký tự' },
          ]}
        >
          <Input placeholder="Nhập tên người ký" />
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
