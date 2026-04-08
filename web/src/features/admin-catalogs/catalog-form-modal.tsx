import { Modal, Form, Input, Switch } from 'antd';
import type { CatalogItemDto, CreateCatalogRequest, UpdateCatalogRequest } from './catalog-types';

interface CatalogFormModalProps {
  open: boolean;
  editingItem: CatalogItemDto | null; // null = create mode
  saving: boolean;
  onSubmit: (values: CreateCatalogRequest | (UpdateCatalogRequest & { id: string })) => void;
  onCancel: () => void;
}

// Modal form for creating/editing a generic catalog item (Code + Name + IsActive)
export function CatalogFormModal({
  open,
  editingItem,
  saving,
  onSubmit,
  onCancel,
}: CatalogFormModalProps) {
  const [form] = Form.useForm();
  const isEdit = Boolean(editingItem);

  function handleOpen(isOpen: boolean) {
    if (isOpen && editingItem) {
      form.setFieldsValue(editingItem);
    } else if (isOpen) {
      form.resetFields();
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
      title={isEdit ? 'Chỉnh sửa danh mục' : 'Thêm mới danh mục'}
      onCancel={onCancel}
      onOk={handleOk}
      okText={isEdit ? 'Lưu thông tin' : 'Thêm mới'}
      cancelText="Quay lại"
      confirmLoading={saving}
      destroyOnHidden
      afterOpenChange={handleOpen}
      width={480}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="code"
          label="Mã danh mục"
          rules={[
            { required: true, message: 'Vui lòng nhập mã danh mục' },
            { max: 50, message: 'Mã danh mục tối đa 50 ký tự' },
          ]}
        >
          <Input placeholder="Nhập mã danh mục" disabled={isEdit} />
        </Form.Item>
        <Form.Item
          name="name"
          label="Tên danh mục"
          rules={[
            { required: true, message: 'Vui lòng nhập tên danh mục' },
            { max: 500, message: 'Tên danh mục tối đa 500 ký tự' },
          ]}
        >
          <Input placeholder="Nhập tên danh mục" />
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
