// RoleFormDrawer — Ant Design Drawer with Form for creating or editing a role.
// create mode: all fields editable. edit mode: code field disabled (system-assigned).

import { useEffect } from 'react';
import { Drawer, Form, Input, Button, Space, message } from 'antd';
import { useTranslation } from 'react-i18next';
import { useCreateRole, useUpdateRole } from './roles-api';
import type { RoleDefinitionDto } from './roles-api';

// ─── Types ────────────────────────────────────────────────────────────────────

interface RoleFormValues {
  code: string;
  name: string;
  description?: string;
}

interface RoleFormDrawerProps {
  open: boolean;
  mode: 'create' | 'edit';
  /** Populated when mode === 'edit' */
  role?: RoleDefinitionDto | null;
  onClose: () => void;
  onSuccess?: () => void;
}

// ─── Component ────────────────────────────────────────────────────────────────

export function RoleFormDrawer({ open, mode, role, onClose, onSuccess }: RoleFormDrawerProps) {
  const { t } = useTranslation();
  const [form] = Form.useForm<RoleFormValues>();

  const createMutation = useCreateRole();
  // useUpdateRole requires ID at hook call time — use empty string when creating
  const updateMutation = useUpdateRole(role?.id ?? '');

  const isPending = createMutation.isPending || updateMutation.isPending;
  const isEdit = mode === 'edit';

  // Populate form when opening in edit mode, reset on create
  useEffect(() => {
    if (!open) return;
    if (isEdit && role) {
      form.setFieldsValue({
        code: role.code,
        name: role.name,
        description: role.description ?? '',
      });
    } else {
      form.resetFields();
    }
  }, [open, isEdit, role, form]);

  async function handleSubmit() {
    try {
      const values = await form.validateFields();
      if (isEdit) {
        await updateMutation.mutateAsync({
          name: values.name,
          description: values.description,
        });
        message.success(t('roles.form.updateSuccess', 'Cập nhật vai trò thành công'));
      } else {
        await createMutation.mutateAsync({
          code: values.code,
          name: values.name,
          description: values.description,
        });
        message.success(t('roles.form.createSuccess', 'Tạo vai trò thành công'));
      }
      onSuccess?.();
      onClose();
    } catch {
      // Form validation errors display inline; API errors shown by interceptor
    }
  }

  const title = isEdit
    ? t('roles.form.editTitle', 'Chỉnh sửa vai trò')
    : t('roles.form.createTitle', 'Tạo vai trò mới');

  return (
    <Drawer
      title={title}
      open={open}
      onClose={onClose}
      width={480}
      destroyOnHidden
      footer={
        <Space style={{ justifyContent: 'flex-end', width: '100%' }}>
          <Button onClick={onClose} disabled={isPending}>
            {t('common.cancel', 'Hủy')}
          </Button>
          <Button type="primary" onClick={handleSubmit} loading={isPending}>
            {isEdit ? t('common.save', 'Lưu') : t('common.create', 'Tạo mới')}
          </Button>
        </Space>
      }
    >
      <Form form={form} layout="vertical" style={{ marginTop: 8 }}>
        <Form.Item
          name="code"
          label={t('roles.form.code', 'Mã vai trò')}
          rules={[{ required: true, message: t('roles.form.codeRequired', 'Vui lòng nhập mã vai trò') }]}
        >
          <Input
            placeholder="VD: GovOfficer"
            disabled={isEdit}
            autoFocus={!isEdit}
          />
        </Form.Item>

        <Form.Item
          name="name"
          label={t('roles.form.name', 'Tên vai trò')}
          rules={[{ required: true, message: t('roles.form.nameRequired', 'Vui lòng nhập tên vai trò') }]}
        >
          <Input placeholder={t('roles.form.namePlaceholder', 'VD: Cán bộ nhà nước')} />
        </Form.Item>

        <Form.Item
          name="description"
          label={t('roles.form.description', 'Mô tả')}
        >
          <Input.TextArea
            rows={3}
            placeholder={t('roles.form.descriptionPlaceholder', 'Mô tả ngắn về vai trò này...')}
          />
        </Form.Item>
      </Form>
    </Drawer>
  );
}
