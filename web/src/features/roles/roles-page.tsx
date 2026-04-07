import { Table, Tag, Typography, Descriptions, Space } from 'antd';
import {
  SafetyCertificateOutlined,
  TeamOutlined,
  UserOutlined,
  CrownOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import { useRoles, type RoleDefinitionDto } from './roles-api';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';

const { Text } = Typography;

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

const columns = [
  {
    title: '',
    key: 'icon',
    width: 48,
    render: (_: unknown, r: RoleDefinitionDto) => (
      <span style={{ fontSize: 20 }}>{ROLE_ICONS[r.name] ?? <SafetyCertificateOutlined />}</span>
    ),
  },
  {
    title: 'Role',
    key: 'name',
    render: (_: unknown, r: RoleDefinitionDto) => (
      <Space direction="vertical" size={0}>
        <Tag color={ROLE_COLORS[r.name] ?? 'default'}>{r.name}</Tag>
        <Text type="secondary" style={{ fontSize: 12 }}>{r.description ?? ''}</Text>
      </Space>
    ),
  },
];

// Roles overview page — displays roles from DB
export function RolesPage() {
  const { t } = useTranslation();
  const { data: roles = [], isLoading } = useRoles();

  return (
    <>
      <AdminPageHeader title={t('roles.title')} />

      <AdminContentCard>
        <Descriptions title={t('roles.authModel')} column={1} size="small">
          <Descriptions.Item label="RBAC">{t('roles.rbacDesc')}</Descriptions.Item>
          <Descriptions.Item label="ABAC">{t('roles.abacDesc')}</Descriptions.Item>
        </Descriptions>
      </AdminContentCard>

      <div style={{ marginTop: 'var(--page-gap, 24px)' }}>
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
      </div>
    </>
  );
}
