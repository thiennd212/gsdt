import React from 'react';
import { Badge, Menu } from 'antd';
import type { ItemType, MenuItemType } from 'antd/es/menu/interface';
import {
  DashboardOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
  AuditOutlined,
  ApiOutlined,
  FileTextOutlined,
  InboxOutlined,
  FormOutlined,
  FolderOutlined,
  BellOutlined,
  RobotOutlined,
  UserOutlined,
  ThunderboltOutlined,
  MessageOutlined,
  FileProtectOutlined,
  SearchOutlined,
  SolutionOutlined,
  TeamOutlined,
} from '@ant-design/icons';
import { useNavigate, useLocation } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import { useThemeMode } from '@/core/theme/use-theme-mode';
import {
  type MenuEntry,
  type MenuItem,
  type MenuGroup,
  getAdminMenuChildren,
} from './admin-menu-entries';

// Top-level menu structure — labels use i18n translation keys
function getMenuEntries(t: (key: string) => string): MenuEntry[] {
  return [
    { key: '/', label: t('nav.dashboard'), icon: <DashboardOutlined /> },
    { key: '/cases', label: t('nav.cases'), icon: <FileTextOutlined /> },
    { key: '/inbox', label: t('nav.inbox'), icon: <InboxOutlined /> },
    { key: '/forms', label: t('nav.forms'), icon: <FormOutlined /> },
    { key: '/files', label: t('nav.files'), icon: <FolderOutlined /> },
    { key: '/notifications', label: t('nav.notifications'), icon: <BellOutlined />, badge: 3 },
    { key: '/ai/search', label: t('nav.aiSearch'), icon: <RobotOutlined /> },
    { key: '/copilot', label: t('nav.copilot'), icon: <ThunderboltOutlined />, badge: 'New' },
    { key: '/chat', label: t('nav.chat'), icon: <MessageOutlined /> },
    { key: '/signatures', label: t('nav.signatures'), icon: <FileProtectOutlined /> },
    {
      key: 'integration',
      label: t('nav.integration'),
      icon: <ApiOutlined />,
      children: [
        { key: '/integration/partners', label: t('nav.integrationPartners'), icon: <TeamOutlined /> },
        { key: '/integration/contracts', label: t('nav.integrationContracts'), icon: <FileTextOutlined /> },
        { key: '/integration/message-logs', label: t('nav.integrationMessageLogs'), icon: <MessageOutlined /> },
      ],
    },
    { key: '/search', label: t('nav.search'), icon: <SearchOutlined /> },
    { key: '/roles', label: t('nav.roles'), icon: <SafetyCertificateOutlined />, requiredPermission: 'roles.read' },
    { key: '/audit/logs', label: t('nav.audit'), icon: <AuditOutlined />, requiredPermission: 'audit.read' },
    {
      key: 'admin',
      label: t('nav.admin'),
      icon: <SettingOutlined />,
      requiredPermission: 'admin.read',
      children: getAdminMenuChildren(t),
    },
    { key: '/consent', label: t('nav.consent'), icon: <SolutionOutlined /> },
    { key: '/profile', label: t('nav.profile'), icon: <UserOutlined /> },
  ];
}

interface SidebarMenuProps {
  permissions: string[];
  roles?: string[];
  // Optional callback invoked after navigation — used by MobileNavDrawer to close itself
  onNavigate?: () => void;
  // When true, submenus open as flyout popups instead of inline expansion
  popupSubMenus?: boolean;
  // When true, sidebar is collapsed — show icons only
  collapsed?: boolean;
}

// Permission-to-role mapping: if user has Admin/SystemAdmin role, grant admin.* permissions
function hasAccess(perm: string, permissions: string[], roles: string[]): boolean {
  if (permissions.includes(perm)) return true;
  // Admin/SystemAdmin roles grant all admin.* permissions
  if (perm.startsWith('admin.') && (roles.includes('Admin') || roles.includes('SystemAdmin'))) return true;
  // GovOfficer role grants audit/roles read
  if (
    (perm === 'audit.read' || perm === 'roles.read') &&
    (roles.includes('GovOfficer') || roles.includes('Admin') || roles.includes('SystemAdmin'))
  ) return true;
  return false;
}

// Wrap menu label with a Badge if badge value is provided
function badgeLabel(label: string, badge?: string | number): React.ReactNode {
  if (badge == null) return label;
  return (
    <>
      {label}
      <Badge
        count={badge}
        size="small"
        style={{
          backgroundColor: typeof badge === 'string' ? '#28A745' : '#007BFF',
          fontSize: 10,
          fontWeight: 600,
          marginLeft: 8,
        }}
      />
    </>
  );
}

// Build Ant Design items from MenuEntry[], filtering by permissions + roles
// Handles: MenuItem (leaf), MenuGroup (submenu), MenuItemGroup (type:'group' visual header)
function buildItems(entries: readonly (MenuEntry | { key: string; label: string; type: 'group'; children: MenuItem[] })[], permissions: string[], roles: string[]): ItemType<MenuItemType>[] {
  return entries
    .filter((e) => !('requiredPermission' in e) || !e.requiredPermission || hasAccess(e.requiredPermission, permissions, roles))
    .flatMap((e): ItemType<MenuItemType>[] => {
      // type:'group' — Ant Menu ItemGroup (visual section header, no submenu depth)
      if ('type' in e && e.type === 'group') {
        const groupChildren = buildItems(e.children, permissions, roles);
        if (groupChildren.length === 0) return [];
        return [{ key: e.key, label: e.label, type: 'group' as const, children: groupChildren }];
      }
      if (!('children' in e)) {
        const item = e as MenuItem;
        return [{ key: item.key, label: badgeLabel(item.label, item.badge), icon: item.icon }];
      }
      const group = e as MenuGroup;
      const visibleChildren = buildItems(group.children as MenuEntry[], permissions, roles);
      if (visibleChildren.length === 0) return [];
      return [{ key: group.key, label: badgeLabel(group.label, group.badge), icon: group.icon, children: visibleChildren }];
    });
}

// SidebarMenu: renders Ant Menu filtered by user permissions + roles, with 4 admin sub-categories
export function SidebarMenu({ permissions, roles = [], onNavigate, popupSubMenus = false, collapsed = false }: SidebarMenuProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { isDark } = useThemeMode();

  const items = buildItems(getMenuEntries(t), permissions, roles);

  // Determine which sub-menus should be open by default based on current path
  // Admin categories are now type:'group' (visual headers) — only 'admin' submenu needs opening.
  const defaultOpenKeys: string[] = location.pathname.startsWith('/admin')
    ? ['admin']
    : location.pathname.startsWith('/integration')
    ? ['integration']
    : [];

  // Use closest-match for selectedKeys so /cases/123 highlights /cases
  const selectedKey =
    location.pathname.startsWith('/cases/') ? '/cases' :
    location.pathname.startsWith('/forms/') ? '/forms' :
    location.pathname.startsWith('/ai/') ? '/ai/search' :
    location.pathname.startsWith('/admin/rules/') ? '/admin/rules' :
    location.pathname.startsWith('/integration/partners') ? '/integration/partners' :
    location.pathname.startsWith('/integration/contracts') ? '/integration/contracts' :
    location.pathname.startsWith('/integration/message-logs') ? '/integration/message-logs' :
    location.pathname;

  return (
    <nav role="navigation" aria-label="Main navigation">
      <div className="gov-sidebar-scroll" style={{ flex: 1, overflowY: 'auto', overflowX: 'hidden' }}>
        <Menu
          theme={isDark ? 'dark' : 'light'}
          mode={popupSubMenus ? 'vertical' : 'inline'}
          inlineCollapsed={collapsed}
          selectedKeys={[selectedKey]}
          defaultOpenKeys={collapsed ? [] : defaultOpenKeys}
          items={items}
          onClick={({ key }) => { navigate({ to: key }); onNavigate?.(); }}
          style={{ border: 'none' }}
        />
      </div>
    </nav>
  );
}
