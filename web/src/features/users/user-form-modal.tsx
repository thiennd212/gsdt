import { useEffect } from 'react';
import { Modal, Form, Input, Select, TreeSelect, message } from 'antd';
import { useTranslation } from 'react-i18next';
import { useCreateUser, useUpdateUser, useAssignStaff } from './user-api';
import { useOrgUnits } from '@/features/organization/org-api';
import { UserRole } from './user-types';
import type { UserDto, CreateUserRequest, UpdateUserRequest } from './user-types';
import type { OrgUnitDto } from '@/features/organization/org-types';

const ROLE_OPTIONS = Object.values(UserRole).map((r) => ({ label: r, value: r }));

// Common gov role-in-org options for Vietnamese government structure
const ROLE_IN_ORG_OPTIONS = [
  'Lãnh đạo', 'Trưởng phòng', 'Phó trưởng phòng',
  'Chuyên viên chính', 'Chuyên viên', 'Nhân viên',
].map((r) => ({ label: r, value: r }));

// Build tree data from flat org unit list for TreeSelect
function buildOrgTree(units: OrgUnitDto[]) {
  const map = new Map<string, { value: string; title: string; children: unknown[] }>();
  const roots: { value: string; title: string; children: unknown[] }[] = [];

  for (const u of units) {
    map.set(u.id, { value: u.id, title: `${u.name} (${u.code})`, children: [] });
  }
  for (const u of units) {
    const node = map.get(u.id)!;
    if (u.parentId && map.has(u.parentId)) {
      map.get(u.parentId)!.children.push(node);
    } else {
      roots.push(node);
    }
  }
  return roots;
}

interface UserFormModalProps {
  open: boolean;
  editUser?: UserDto | null;
  onClose: () => void;
}

// UserFormModal — create or edit a user (name, email, roles, department, org unit assignment)
export function UserFormModal({ open, editUser, onClose }: UserFormModalProps) {
  const { t } = useTranslation();
  const [form] = Form.useForm<CreateUserRequest & { orgUnitId?: string; roleInOrg?: string }>();
  const isEdit = Boolean(editUser);

  const createMutation = useCreateUser();
  const updateMutation = useUpdateUser(editUser?.id ?? '');
  const assignStaffMutation = useAssignStaff();
  const { data: orgUnits = [] } = useOrgUnits();

  const isPending = createMutation.isPending || updateMutation.isPending || assignStaffMutation.isPending;
  const treeData = buildOrgTree(orgUnits);

  // Populate form when editing
  useEffect(() => {
    if (open && editUser) {
      form.setFieldsValue({
        fullName: editUser.fullName,
        email: editUser.email,
        department: editUser.departmentCode,
        roles: editUser.roles,
      });
    } else if (open) {
      form.resetFields();
    }
  }, [open, editUser, form]);

  async function handleOk() {
    try {
      const values = await form.validateFields();
      if (isEdit) {
        const body: UpdateUserRequest = {
          fullName: values.fullName,
          department: values.department,
          orgUnitId: values.orgUnitId,
          roles: values.roles,
        };
        await updateMutation.mutateAsync(body);
        message.success(t('page.admin.users.updateSuccess'));
      } else {
        // Step 1: Create user
        const newUser = await createMutation.mutateAsync(values);

        // Step 2: Assign to org unit if selected
        if (values.orgUnitId && values.roleInOrg && newUser) {
          const userId = typeof newUser === 'string' ? newUser : (newUser as { id?: string })?.id;
          if (userId) {
            await assignStaffMutation.mutateAsync({
              userId,
              orgUnitId: values.orgUnitId,
              roleInOrg: values.roleInOrg,
            });
          }
        }
        message.success(t('page.admin.users.createSuccess'));
      }
      onClose();
    } catch {
      // validation or API error — antd form shows inline errors
    }
  }

  return (
    <Modal
      title={isEdit ? t('page.admin.users.editTitle') : t('page.admin.users.createTitle')}
      open={open}
      onOk={handleOk}
      onCancel={onClose}
      okText={isEdit ? t('common.save') : t('page.admin.users.createOkBtn')}
      cancelText={t('common.cancel')}
      confirmLoading={isPending}
      destroyOnHidden
      width={560}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="fullName"
          label={t('page.admin.users.col.fullName')}
          rules={[{ required: true, message: t('page.admin.users.fullNameRequired') }]}
        >
          <Input placeholder={t('page.admin.users.fullNamePlaceholder')} />
        </Form.Item>

        <Form.Item
          name="email"
          label={t('page.admin.users.col.email')}
          rules={[
            { required: true, message: t('page.admin.users.emailRequired') },
            { type: 'email', message: t('page.admin.users.emailInvalid') },
          ]}
        >
          <Input placeholder="example@gov.vn" disabled={isEdit} />
        </Form.Item>

        <Form.Item
          name="roles"
          label={t('page.admin.users.col.roles')}
          rules={[{ required: true, message: t('page.admin.users.rolesRequired') }]}
        >
          <Select mode="multiple" placeholder={t('page.admin.users.rolesPlaceholder')} options={ROLE_OPTIONS} />
        </Form.Item>

        {/* Org unit assignment — TreeSelect shows org hierarchy */}
        <Form.Item name="orgUnitId" label="Đơn vị / Phòng ban">
          <TreeSelect
            treeData={treeData}
            placeholder="Chọn đơn vị"
            allowClear
            showSearch
            treeNodeFilterProp="title"
            treeDefaultExpandAll
          />
        </Form.Item>

        <Form.Item name="roleInOrg" label="Vai trò trong đơn vị">
          <Select
            placeholder="Chọn vai trò"
            options={ROLE_IN_ORG_OPTIONS}
            allowClear
          />
        </Form.Item>

        <Form.Item name="department" label={t('page.admin.users.departmentLabel')}>
          <Input placeholder={t('page.admin.users.departmentPlaceholder')} />
        </Form.Item>

        {!isEdit && (
          <Form.Item name="temporaryPassword" label={t('page.admin.users.tempPasswordLabel')}>
            <Input.Password placeholder={t('page.admin.users.tempPasswordPlaceholder')} />
          </Form.Item>
        )}
      </Form>
    </Modal>
  );
}
