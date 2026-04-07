import { Row, Col, Alert, Spin, Typography, Button, Skeleton, Space } from 'antd';
import { ReloadOutlined, CalendarOutlined } from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import { lazy, Suspense, useState } from 'react';
import { useDashboardKpi } from './dashboard-api';
import { KpiStatCards } from './kpi-stat-cards';
import { TopAssigneesTable } from './top-assignees-table';
import { useAnnouncements } from '../system-params/system-params-api';
import type { AnnouncementDto } from '../system-params/system-params-types';
import { useAuth } from '@/features/auth';
import { GOV_COLORS } from '@/app/theme';

// Chart components lazy-loaded so echarts chunk is deferred until dashboard is visited
const CasesByStatusChart = lazy(() =>
  import('./cases-by-status-chart').then((m) => ({ default: m.CasesByStatusChart })),
);
const CasesByTypeChart = lazy(() =>
  import('./cases-by-type-chart').then((m) => ({ default: m.CasesByTypeChart })),
);
const MonthlyTrendChart = lazy(() =>
  import('./monthly-trend-chart').then((m) => ({ default: m.MonthlyTrendChart })),
);

const { Title } = Typography;

// activeAnnouncements — filter to status=Active and within date range (if set)
function activeAnnouncements(items: AnnouncementDto[]): AnnouncementDto[] {
  const now = Date.now();
  return items.filter((a) => {
    if (a.status !== 'Active') return false;
    if (a.startDate && new Date(a.startDate).getTime() > now) return false;
    if (a.endDate && new Date(a.endDate).getTime() < now) return false;
    return true;
  });
}

// DashboardPage — KPI overview with charts; auto-refreshes every 5 minutes
export function DashboardPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const { data, isFetching, isLoading, isError, refetch } = useDashboardKpi();
  const { data: allAnnouncements = [] } = useAnnouncements();
  const displayName = user?.profile?.name ?? 'Admin';
  const today = new Date().toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
  const [dismissedIds, setDismissedIds] = useState<Set<string>>(new Set());
  const banners = activeAnnouncements(allAnnouncements).filter((a) => !dismissedIds.has(a.id));

  if (isLoading) {
    return (
      <Spin size="large" tip={t('page.dashboard.loading')}>
        <div style={{ minHeight: 200 }} />
      </Spin>
    );
  }

  if (isError || !data) {
    return (
      <Alert
        type="error"
        showIcon
        message={t('page.dashboard.errorMessage')}
        description={t('page.dashboard.errorDescription')}
        action={
          <Button size="small" onClick={() => refetch()}>
            {t('page.dashboard.retry')}
          </Button>
        }
      />
    );
  }

  return (
    <div>
      {/* Page header — greeting + date + refresh */}
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', marginBottom: 24 }}>
        <div>
          <Title level={3} style={{ margin: 0, fontWeight: 700 }}>
            {t('page.dashboard.greeting', { name: displayName, defaultValue: `Xin chào, ${displayName}` })}
          </Title>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: 4, color: GOV_COLORS.textSecondary, fontSize: 13 }}>
            <CalendarOutlined />
            <span>{today}</span>
          </div>
        </div>
        <Button
          type="text"
          icon={<ReloadOutlined spin={isFetching} />}
          onClick={() => refetch()}
          style={{ color: 'var(--gov-text-secondary)' }}
        >
          {t('page.dashboard.refresh')}
        </Button>
      </div>

      {/* Active announcement banners */}
      {banners.length > 0 && (
        <Space direction="vertical" style={{ width: '100%', marginBottom: 16 }}>
          {banners.map((a) => (
            <Alert
              key={a.id}
              type="info"
              showIcon
              closable
              message={a.title}
              description={a.content}
              onClose={() => setDismissedIds((prev) => new Set([...prev, a.id]))}
            />
          ))}
        </Space>
      )}

      {/* KPI stat cards — 4 metrics in a row */}
      <KpiStatCards data={data} loading={isFetching} />

      {/* Charts row 1: status donut + type bar — echarts loaded lazily */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} lg={12}>
          <Suspense fallback={<Skeleton.Node active style={{ width: '100%', height: 312 }} />}>
            <CasesByStatusChart data={data.casesByStatus} />
          </Suspense>
        </Col>
        <Col xs={24} lg={12}>
          <Suspense fallback={<Skeleton.Node active style={{ width: '100%', height: 312 }} />}>
            <CasesByTypeChart data={data.casesByType} />
          </Suspense>
        </Col>
      </Row>

      {/* Charts row 2: monthly trend (full width) */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24}>
          <Suspense fallback={<Skeleton.Node active style={{ width: '100%', height: 312 }} />}>
            <MonthlyTrendChart data={data.monthlyTrend} />
          </Suspense>
        </Col>
      </Row>

      {/* Top assignees table */}
      <Row gutter={[16, 16]}>
        <Col xs={24}>
          <TopAssigneesTable data={data.topAssignees} loading={isFetching} />
        </Col>
      </Row>
    </div>
  );
}
