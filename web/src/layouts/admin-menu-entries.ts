import {
  TeamOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
  AuditOutlined,
  DatabaseOutlined,
  ApartmentOutlined,
  KeyOutlined,
  BellOutlined,
  HeartOutlined,
  BookOutlined,
  ApiOutlined,
  CloudUploadOutlined,
  SwapOutlined,
  DesktopOutlined,
  NodeIndexOutlined,
  EditOutlined,
  FileProtectOutlined,
  ScheduleOutlined,
  ExperimentOutlined,
  MenuOutlined,
  UsergroupAddOutlined,
  LinkOutlined,
  EyeOutlined,
  BlockOutlined,
  StopOutlined,
  DeleteOutlined,
  LockOutlined,
  SafetyOutlined,
  DashboardOutlined,
} from '@ant-design/icons';
import React from 'react';

// Flat menu item (leaf node)
export interface MenuItem {
  key: string;
  label: string;
  icon: React.ReactNode;
  requiredPermission?: string;
  badge?: string | number; // e.g. "New", 3
}

// Group menu item (sub-menu) — supports recursive nesting (MenuItem | MenuGroup)
export interface MenuGroup {
  key: string;
  label: string;
  icon: React.ReactNode;
  requiredPermission?: string;
  children: (MenuItem | MenuGroup)[];
  badge?: string | number;
}

export type MenuEntry = MenuItem | MenuGroup;

// Paths belonging to Identity & Access category
export const IDENTITY_PATHS = [
  '/admin/users',
  '/admin/groups',
  '/admin/external-identities',
  '/admin/jit-provider-configs',
  '/admin/abac-rules',
  '/admin/sod-rules',
  '/admin/policy-rules',
  '/admin/credential-policies',
  '/admin/access-reviews',
  '/admin/delegations',
  '/admin/sessions',
];

// Paths belonging to Content & Workflow category
export const CONTENT_PATHS = [
  '/admin/workflow',
  '/admin/rules',
  '/admin/templates',
  '/admin/notification-templates',
  '/admin/menus',
];

// Paths belonging to System category
export const SYSTEM_PATHS = [
  '/admin/system-params',
  '/admin/master-data',
  '/admin/catalogs',
  '/admin/organization',
  '/admin/jobs',
  '/admin/health',
  '/admin/backup',
  '/admin/rtbf',
];

// Paths belonging to Integration category
export const INTEGRATION_PATHS = [
  '/admin/api-keys',
  '/admin/webhooks',
  '/admin/ai',
  '/admin/data-scopes',
];

// Admin menu item type for Ant Menu ItemGroup (visual-only grouping, no nesting depth)
export interface MenuItemGroup {
  key: string;
  label: string;
  type: 'group';
  children: MenuItem[];
}

// Build admin children as flat items with visual group headers (max 2 levels: Admin → Item)
// Uses Ant Menu type:'group' for category labels — renders as section headers, not submenus.
export function getAdminMenuChildren(t: (key: string) => string): (MenuItem | MenuItemGroup)[] {
  return [
    { key: '/admin', label: t('nav.adminOverview'), icon: React.createElement(DashboardOutlined) },
    {
      key: 'admin-identity', label: t('nav.adminIdentity'), type: 'group' as const,
      children: [
        { key: '/admin/users', label: t('nav.users'), icon: React.createElement(TeamOutlined) },
        { key: '/admin/groups', label: t('nav.groups'), icon: React.createElement(UsergroupAddOutlined) },
        { key: '/admin/external-identities', label: t('nav.externalIdentities'), icon: React.createElement(LinkOutlined) },
        { key: '/admin/jit-provider-configs', label: t('nav.jitProviderConfigs'), icon: React.createElement(ApiOutlined) },
        { key: '/admin/abac-rules', label: t('nav.abacRules'), icon: React.createElement(SafetyCertificateOutlined) },
        { key: '/admin/sod-rules', label: t('nav.sodRules'), icon: React.createElement(BlockOutlined) },
        { key: '/admin/policy-rules', label: t('nav.policyRules'), icon: React.createElement(StopOutlined) },
        { key: '/admin/credential-policies', label: t('nav.credentialPolicies'), icon: React.createElement(LockOutlined) },
        { key: '/admin/access-reviews', label: t('nav.accessReviews'), icon: React.createElement(AuditOutlined) },
        { key: '/admin/delegations', label: t('nav.delegations'), icon: React.createElement(SwapOutlined) },
        { key: '/admin/sessions', label: t('nav.sessions'), icon: React.createElement(DesktopOutlined) },
      ],
    },
    {
      key: 'admin-content', label: t('nav.adminContent'), type: 'group' as const,
      children: [
        { key: '/admin/workflow', label: t('nav.workflow'), icon: React.createElement(NodeIndexOutlined) },
        { key: '/admin/rules', label: t('nav.rules'), icon: React.createElement(EditOutlined) },
        { key: '/admin/templates', label: t('nav.templates'), icon: React.createElement(FileProtectOutlined) },
        { key: '/admin/notification-templates', label: t('nav.notificationTemplates'), icon: React.createElement(BellOutlined) },
        { key: '/admin/menus', label: t('nav.menus'), icon: React.createElement(MenuOutlined) },
      ],
    },
    {
      key: 'admin-system', label: t('nav.adminSystem'), type: 'group' as const,
      children: [
        { key: '/admin/system-params', label: t('nav.systemParams'), icon: React.createElement(SettingOutlined) },
        { key: '/admin/master-data', label: t('nav.masterData'), icon: React.createElement(DatabaseOutlined) },
        { key: '/admin/catalogs', label: t('nav.catalogs'), icon: React.createElement(BookOutlined) },
        { key: '/admin/organization', label: t('nav.organization'), icon: React.createElement(ApartmentOutlined) },
        { key: '/admin/jobs', label: t('nav.jobs'), icon: React.createElement(ScheduleOutlined) },
        { key: '/admin/health', label: t('nav.healthCheck'), icon: React.createElement(HeartOutlined) },
        { key: '/admin/backup', label: t('nav.backup'), icon: React.createElement(CloudUploadOutlined) },
        { key: '/admin/rtbf', label: t('nav.rtbf'), icon: React.createElement(DeleteOutlined) },
      ],
    },
    {
      key: 'admin-integration', label: t('nav.adminIntegration'), type: 'group' as const,
      children: [
        { key: '/admin/api-keys', label: t('nav.apiKeys'), icon: React.createElement(KeyOutlined) },
        { key: '/admin/webhooks', label: t('nav.webhooks'), icon: React.createElement(ApiOutlined) },
        { key: '/admin/ai', label: t('nav.aiAdmin'), icon: React.createElement(ExperimentOutlined) },
        { key: '/admin/data-scopes', label: t('nav.dataScopes'), icon: React.createElement(EyeOutlined) },
      ],
    },
  ];
}

// Determine which admin category the pathname belongs to (used for breadcrumbs)
// Categories are now visual groups (type:'group'), not submenus — no open key needed.
export function getAdminCategoryKey(pathname: string): string | null {
  if (IDENTITY_PATHS.some((p) => pathname.startsWith(p))) return 'admin-identity';
  if (CONTENT_PATHS.some((p) => pathname.startsWith(p))) return 'admin-content';
  if (SYSTEM_PATHS.some((p) => pathname.startsWith(p))) return 'admin-system';
  if (INTEGRATION_PATHS.some((p) => pathname.startsWith(p))) return 'admin-integration';
  return null;
}

// Map each admin route to its category i18n key for breadcrumb
export const ADMIN_ROUTE_CATEGORY: Record<string, string> = {
  ...Object.fromEntries(IDENTITY_PATHS.map((p) => [p, 'nav.adminIdentity'])),
  ...Object.fromEntries(CONTENT_PATHS.map((p) => [p, 'nav.adminContent'])),
  ...Object.fromEntries(SYSTEM_PATHS.map((p) => [p, 'nav.adminSystem'])),
  ...Object.fromEntries(INTEGRATION_PATHS.map((p) => [p, 'nav.adminIntegration'])),
};
