// RolePermissionsPanel — permission matrix for a single role.
// Groups permissions by module in a Collapse layout with checkboxes.
// Tracks local changes and diffs against server state on Save.

import { useState, useMemo, useEffect } from 'react';
import {
  Drawer, Collapse, Checkbox, Button, Space, Input,
  Spin, Typography, Badge, message,
} from 'antd';
import { SearchOutlined, SaveOutlined } from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import { usePermissionsByModule, useAssignPermissions, useRemovePermissions } from './permissions-api';
import { useRolePermissions } from './permissions-api';
import type { PermissionDto } from './permissions-api';

const { Text } = Typography;

// ─── Types ────────────────────────────────────────────────────────────────────

interface RolePermissionsPanelProps {
  open: boolean;
  roleId: string | null;
  roleName?: string;
  onClose: () => void;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

function buildCheckedSet(permissions: PermissionDto[]): Set<string> {
  return new Set(permissions.map((p) => p.id));
}

// ─── Component ────────────────────────────────────────────────────────────────

export function RolePermissionsPanel({ open, roleId, roleName, onClose }: RolePermissionsPanelProps) {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  // Local checked state — tracks in-flight edits before Save
  const [localChecked, setLocalChecked] = useState<Set<string>>(new Set());
  const [isDirty, setIsDirty] = useState(false);

  const { data: modules = [], isLoading: modulesLoading } = usePermissionsByModule();
  const { data: assigned = [], isLoading: assignedLoading } = useRolePermissions(roleId);

  const assignMutation = useAssignPermissions(roleId ?? '');
  const removeMutation = useRemovePermissions(roleId ?? '');

  const isSaving = assignMutation.isPending || removeMutation.isPending;
  const isLoading = modulesLoading || assignedLoading;

  // Sync local state when server data arrives (reset dirty flag)
  useEffect(() => {
    if (!assignedLoading) {
      setLocalChecked(buildCheckedSet(assigned));
      setIsDirty(false);
    }
  }, [assigned, assignedLoading]);

  // Filter modules/permissions by search query
  const filteredModules = useMemo(() => {
    if (!search.trim()) return modules;
    const q = search.toLowerCase();
    return modules
      .map((m) => ({
        ...m,
        permissions: m.permissions.filter(
          (p) =>
            p.name.toLowerCase().includes(q) ||
            p.code.toLowerCase().includes(q) ||
            m.moduleCode.toLowerCase().includes(q),
        ),
      }))
      .filter((m) => m.permissions.length > 0);
  }, [modules, search]);

  function handleToggle(permId: string, checked: boolean) {
    setLocalChecked((prev) => {
      const next = new Set(prev);
      if (checked) next.add(permId); else next.delete(permId);
      return next;
    });
    setIsDirty(true);
  }

  function handleModuleToggle(permIds: string[], allChecked: boolean) {
    setLocalChecked((prev) => {
      const next = new Set(prev);
      if (allChecked) {
        permIds.forEach((id) => next.delete(id));
      } else {
        permIds.forEach((id) => next.add(id));
      }
      return next;
    });
    setIsDirty(true);
  }

  async function handleSave() {
    if (!roleId) return;
    const serverSet = buildCheckedSet(assigned);

    // Diff: what to add vs remove
    const toAdd = [...localChecked].filter((id) => !serverSet.has(id));
    const toRemove = [...serverSet].filter((id) => !localChecked.has(id));

    try {
      if (toAdd.length > 0) {
        await assignMutation.mutateAsync({ permissionIds: toAdd });
      }
      if (toRemove.length > 0) {
        await removeMutation.mutateAsync({ permissionIds: toRemove });
      }
      message.success(t('roles.permissions.saveSuccess', 'Cập nhật quyền thành công'));
      setIsDirty(false);
    } catch {
      // API error displayed by global interceptor
    }
  }

  const collapseItems = filteredModules.map((module) => {
    const permIds = module.permissions.map((p) => p.id);
    const checkedInModule = permIds.filter((id) => localChecked.has(id));
    const allChecked = checkedInModule.length === permIds.length;
    const someChecked = checkedInModule.length > 0 && !allChecked;

    return {
      key: module.moduleCode,
      label: (
        <Space>
          <Checkbox
            checked={allChecked}
            indeterminate={someChecked}
            onClick={(e) => {
              e.stopPropagation();
              handleModuleToggle(permIds, allChecked);
            }}
          />
          <Text strong>{module.moduleCode}</Text>
          <Badge count={checkedInModule.length} showZero color={checkedInModule.length > 0 ? 'blue' : 'default'} />
        </Space>
      ),
      children: (
        <Space direction="vertical" style={{ width: '100%', paddingLeft: 24 }}>
          {module.permissions.map((perm) => (
            <Checkbox
              key={perm.id}
              checked={localChecked.has(perm.id)}
              onChange={(e) => handleToggle(perm.id, e.target.checked)}
            >
              <Space direction="vertical" size={0}>
                <Text>{perm.name}</Text>
                <Text type="secondary" style={{ fontSize: 11 }}>{perm.code}</Text>
              </Space>
            </Checkbox>
          ))}
        </Space>
      ),
    };
  });

  return (
    <Drawer
      title={`${t('roles.permissions.title', 'Phân quyền')}: ${roleName ?? ''}`}
      open={open}
      onClose={onClose}
      width={560}
      destroyOnHidden
      footer={
        <Space style={{ justifyContent: 'flex-end', width: '100%' }}>
          <Button onClick={onClose}>{t('common.cancel', 'Đóng')}</Button>
          <Button
            type="primary"
            icon={<SaveOutlined />}
            loading={isSaving}
            disabled={!isDirty}
            onClick={handleSave}
          >
            {t('common.save', 'Lưu thay đổi')}
          </Button>
        </Space>
      }
    >
      {isLoading ? (
        <Spin style={{ display: 'block', margin: '40px auto' }} />
      ) : (
        <Space direction="vertical" style={{ width: '100%' }} size="middle">
          <Input
            placeholder={t('roles.permissions.search', 'Tìm kiếm quyền...')}
            prefix={<SearchOutlined />}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            allowClear
          />
          {collapseItems.length === 0 ? (
            <Text type="secondary">{t('roles.permissions.noResults', 'Không tìm thấy quyền phù hợp')}</Text>
          ) : (
            <Collapse items={collapseItems} defaultActiveKey={filteredModules.map((m) => m.moduleCode)} />
          )}
        </Space>
      )}
    </Drawer>
  );
}
