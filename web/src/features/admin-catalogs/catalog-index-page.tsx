import { Card, Col, Row, Typography } from 'antd';
import { useNavigate } from '@tanstack/react-router';
import { ScheduleOutlined } from '@ant-design/icons';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { CATALOG_CONFIG, KHLCNT_META } from './catalog-config';

const { Text } = Typography;

// Navigation hub for all 11 dynamic catalogs (10 generic + KHLCNT)
export function CatalogIndexPage() {
  const navigate = useNavigate();

  const genericEntries = Object.entries(CATALOG_CONFIG);

  return (
    <div>
      <AdminPageHeader
        title="Quản lý danh mục"
        description="Quản lý các danh mục dùng chung cho dự án đầu tư công"
        icon={<ScheduleOutlined />}
        stats={{ total: genericEntries.length + 1, label: 'danh mục' }}
      />
      <Row gutter={[16, 16]}>
        {genericEntries.map(([slug, meta]) => (
          <Col xs={24} sm={12} lg={8} xl={6} key={slug}>
            <Card
              hoverable
              size="small"
              onClick={() => navigate({ to: `/admin/catalogs/${slug}` })}
              style={{ borderRadius: 12, height: '100%' }}
              styles={{ body: { padding: '16px 20px' } }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
                <span style={{ fontSize: 18, color: 'var(--gov-navy)' }}>{meta.icon}</span>
                <Text strong style={{ fontSize: 14 }}>{meta.label}</Text>
              </div>
              <Text type="secondary" style={{ fontSize: 12 }}>{meta.description}</Text>
            </Card>
          </Col>
        ))}
        {/* KHLCNT card — dedicated page */}
        <Col xs={24} sm={12} lg={8} xl={6}>
          <Card
            hoverable
            size="small"
            onClick={() => navigate({ to: '/admin/catalogs/contractor-selection-plans' })}
            style={{ borderRadius: 12, height: '100%', borderColor: 'var(--gov-navy)' }}
            styles={{ body: { padding: '16px 20px' } }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
              <span style={{ fontSize: 18, color: 'var(--gov-navy)' }}>{KHLCNT_META.icon}</span>
              <Text strong style={{ fontSize: 14 }}>{KHLCNT_META.label}</Text>
            </div>
            <Text type="secondary" style={{ fontSize: 12 }}>{KHLCNT_META.description}</Text>
          </Card>
        </Col>
      </Row>
    </div>
  );
}
