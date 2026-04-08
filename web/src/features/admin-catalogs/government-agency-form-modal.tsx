import { Modal, Form, Input, InputNumber, Select, Switch, Row, Col } from 'antd';
import { useGovernmentAgencyTree } from './catalog-api';
import type {
  GovernmentAgencyDto,
  GovernmentAgencyTreeNode,
  CreateGovernmentAgencyRequest,
  UpdateGovernmentAgencyRequest,
} from './catalog-types';

// Agency type options (Loại cơ quan)
const AGENCY_TYPE_OPTIONS = [
  { value: 'Các Tỉnh', label: 'Các Tỉnh' },
  { value: 'Các Bộ, Ban ngành', label: 'Các Bộ, Ban ngành' },
  { value: 'Các Quận/huyện', label: 'Các Quận/huyện' },
  { value: 'Các Tổng công ty', label: 'Các Tổng công ty' },
];

// Flatten tree nodes into a list for parent TreeSelect options
function flattenTree(
  nodes: GovernmentAgencyTreeNode[],
  depth = 0,
): { value: string; label: string }[] {
  return nodes.flatMap((node) => [
    { value: node.id, label: `${'— '.repeat(depth)}${node.name} (${node.code})` },
    ...flattenTree(node.children, depth + 1),
  ]);
}

interface GovernmentAgencyFormModalProps {
  open: boolean;
  editingItem: GovernmentAgencyDto | null;
  saving: boolean;
  onSubmit: (
    values:
      | CreateGovernmentAgencyRequest
      | (UpdateGovernmentAgencyRequest & { id: string }),
  ) => void;
  onCancel: () => void;
}

// Modal form for GovernmentAgency — handles both create and edit modes
export function GovernmentAgencyFormModal({
  open,
  editingItem,
  saving,
  onSubmit,
  onCancel,
}: GovernmentAgencyFormModalProps) {
  const [form] = Form.useForm();
  const isEdit = Boolean(editingItem);
  const { data: tree = [] } = useGovernmentAgencyTree();
  const parentOptions = flattenTree(tree);

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
      title={isEdit ? 'Chỉnh sửa cơ quan' : 'Thêm mới cơ quan'}
      onCancel={onCancel}
      onOk={handleOk}
      okText={isEdit ? 'Lưu thông tin' : 'Thêm mới'}
      cancelText="Quay lại"
      confirmLoading={saving}
      destroyOnHidden
      afterOpenChange={handleOpen}
      width={680}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Row gutter={16}>
          <Col span={14}>
            <Form.Item
              name="name"
              label="Tên cơ quan"
              rules={[{ required: true, message: 'Vui lòng nhập tên cơ quan' }]}
            >
              <Input placeholder="Nhập tên cơ quan quản lý nhà nước" />
            </Form.Item>
          </Col>
          <Col span={10}>
            <Form.Item
              name="code"
              label="Mã cơ quan"
              rules={[{ required: true, message: 'Vui lòng nhập mã cơ quan' }]}
            >
              <Input placeholder="Nhập mã cơ quan" />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item name="parentId" label="Cơ quan cấp trên">
              <Select
                allowClear
                showSearch
                placeholder="Chọn cơ quan cấp trên"
                options={parentOptions}
                filterOption={(input, option) =>
                  (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                }
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="agencyType" label="Loại cơ quan">
              <Select
                allowClear
                placeholder="Chọn loại cơ quan"
                options={AGENCY_TYPE_OPTIONS}
              />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item name="address" label="Địa chỉ">
              <Input placeholder="Nhập địa chỉ" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="email" label="Email">
              <Input placeholder="Nhập email" />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="phone" label="Điện thoại">
              <Input placeholder="Số điện thoại" />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="fax" label="Fax">
              <Input placeholder="Số fax" />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="origin" label="Xuất xứ">
              <Input placeholder="Xuất xứ" />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item name="ldaServer" label="LDA Server">
              <Input placeholder="Địa chỉ LDA Server" />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="sortOrder" label="Thứ tự sắp xếp">
              <InputNumber style={{ width: '100%' }} min={0} placeholder="0" />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="reportDisplayOrder" label="Thứ tự báo cáo">
              <InputNumber style={{ width: '100%' }} min={0} placeholder="0" />
            </Form.Item>
          </Col>
        </Row>
        <Form.Item name="notes" label="Ghi chú">
          <Input.TextArea rows={2} placeholder="Ghi chú thêm" />
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
