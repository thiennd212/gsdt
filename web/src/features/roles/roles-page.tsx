// RolesPage — role management table with Create/Edit/Permissions/Delete actions.
// System roles (roleType === 'System') cannot be deleted.

import { useState } from 'react';
import { Table, Tag, Typography, Space, Button, Modal, Tooltip } from 'antd';
import {
  SafetyCertificateOutlined, TeamOutlined, UserOutlined,
  CrownOutlined, EyeOutlined, PlusOutlined, EditOutlined,
  KeyOutlined, DeleteOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import { useRoles, useDeleteRole, type RoleDefinitionDto } from './roles-api';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { RoleFormDrawer } from './role-form-drawer';
import { RolePermissionsPanel } from './role-permissions-panel';

const { Text } = Typography;

// ─── Static lookup maps ───────────────────────────────────────────────────────

const ROLE_ICONS: Record<string, React.ReactNode> = {
  SystemAdmin: <CrownOutlined />,
  Admin: <SafetyCertificateOutlined />,
  GovOfficer: <TeamOutlined />,
  Citizen: <UserOutlined />,
  Viewer: <EyeOutlined />,
};

const ROLE_COLORS: Record<string, string> = {
  SystemAdmin: 'red',
  Admin: 'volcano',
  GovOfficer: 'blue',
  Citizen: 'green',
  Viewer: 'default',
};

const TYPE_LABELS: Record<string, string> = {
  System: 'Hệ thống',
  Custom: 'Tùy chỉnh',
};

// ─── Component ────────────────────────────────────────────────────────────────

export function RolesPage() {
  const { t } = useTranslation();
  const { data: roles = [], isLoading } = useRoles();
  const deleteMutation = useDeleteRole();

  // Drawer / panel state
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [drawerMode, setDrawerMode] = useState<'create' | 'edit'>('create');
  const [selectedRole, setSelectedRole] = useState<RoleDefinitionDto | null>(null);
  const [permPanelOpen, setPermPanelOpen] = useState(false);
  const [permRole, setPermRole] = useState<RoleDefinitionDto | null>(null);

  function openCreate() {
    setSelectedRole(null);
    setDrawerMode('create');
    setDrawerOpen(true);
  }

  function openEdit(role: RoleDefinitionDto) {
    setSelectedRole(role);
    setDrawerMode('edit');
    setDrawerOpen(true);
  }

  function openPermissions(role: RoleDefinitionDto) {
    setPermRole(role);
    setPermPanelOpen(true);
  }

  function confirmDelete(role: RoleDefinitionDto) {
    Modal.confirm({
      title: t('roles.deleteConfirm.title', 'Xóa vai trò'),
      content: t('roles.deleteConfirm.body', `Bạn chắc chắn muốn xóa vai trò "${role.name}"?`),
      okText: t('common.delete', 'Xóa'),
      okButtonProps: { danger: true },
      cancelText: t('common.cancel', 'Hủy'),
      onOk: () => deleteMutation.mutateAsync(role.id),
    });
  }

  const columns: ColumnsType<RoleDefinitionDto> = [
    {
      title: '',
      key: 'icon',
      width: 44,
      render: (_, r) => (
        <span style={{ fontSize: 18 }}>
          {ROLE_ICONS[r.name] ?? <SafetyCertificateOutlined />}
        </span>
      ),
    },
    {
      title: t('roles.col.code', 'Mã'),
      dataIndex: 'code',
      key: 'code',
      width: 160,
      render: (code: string) => <Text code>{code}</Text>,
    },
    {
      title: t('roles.col.name', 'Tên vai trò'),
      key: 'name',
      render: (_, r) => (
        <Space direction="vertical" size={0}>
          <Tag color={ROLE_COLORS[r.name] ?? 'default'}>{r.name}</Tag>
          {r.description && (
            <Text type="secondary" style={{ fontSize: 12 }}>{r.description}</Text>
          )}
        </Space>
      ),
    },
    {
      title: t('roles.col.type', 'Loại'),
      dataIndex: 'roleType',
      key: 'roleType',
      width: 110,
      render: (type: string) => (
        <Tag color={type === 'System' ? 'purple' : 'cyan'}>
          {TYPE_LABELS[type] ?? type}
        </Tag>
      ),
    },
    {
      title: t('roles.col.permissions', 'Số quyền'),
      dataIndex: 'permissionCount',
      key: 'permissionCount',
      width: 100,
      align: 'center',
      render: (count: number) => <Tag color="blue">{count}</Tag>,
    },
    {
      title: t('roles.col.status', 'Trạng thái'),
      dataIndex: 'isActive',
      key: 'isActive',
      width: 110,
      render: (active: boolean) => (
        <Tag color={active ? 'green' : 'red'}>
          {active ? t('common.active', 'Hoạt động') : t('common.inactive', 'Vô hiệu')}
        </Tag>
      ),
    },
    {
      title: t('common.actions', 'Thao tác'),
      key: 'actions',
      width: 140,
      render: (_, r) => {
        const isSystem = r.roleType === 'System';
        return (
          <Space size="small">
            <Tooltip title={t('common.edit', 'Chỉnh sửa')}>
              <Button
                size="small"
                icon={<EditOutlined />}
                onClick={() => openEdit(r)}
              />
            </Tooltip>
            <Tooltip title={t('roles.managePermissions', 'Phân quyền')}>
              <Button
                size="small"
                icon={<KeyOutlined />}
                onClick={() => openPermissions(r)}
              />
            </Tooltip>
            <Tooltip title={isSystem ? t('roles.cannotDeleteSystem', 'Không thể xóa vai trò hệ thống') : t('common.delete', 'Xóa')}>
              <Button
                size="small"
                danger
                icon={<DeleteOutlined />}
                disabled={isSystem}
                loading={deleteMutation.isPending}
                onClick={() => confirmDelete(r)}
              />
            </Tooltip>
          </Space>
        );
      },
    },
  ];

  return (
    <>
      <AdminPageHeader
        title={t('roles.title', 'Quản lý vai trò')}
        actions={
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
            {t('roles.createBtn', 'Tạo vai trò')}
          </Button>
        }
        stats={{ total: roles.length, label: t('common.roles', 'vai trò') }}
      />

      <AdminContentCard noPadding>
        <Table
          dataSource={roles}
          columns={columns}
          rowKey="id"
          pagination={false}
          bordered
          size="middle"
          loading={isLoading}
        />
      </AdminContentCard>

      {/* Create / Edit drawer */}
      <RoleFormDrawer
        open={drawerOpen}
        mode={drawerMode}
        role={selectedRole}
        onClose={() => setDrawerOpen(false)}
      />

      {/* Permission assignment panel */}
      <RolePermissionsPanel
        open={permPanelOpen}
        roleId={permRole?.id ?? null}
        roleName={permRole?.name}
        onClose={() => setPermPanelOpen(false)}
      />
    </>
  );
}
