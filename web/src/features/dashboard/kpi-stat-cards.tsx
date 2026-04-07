import { Row, Col, Card } from 'antd';
import { memo, useMemo } from 'react';
import {
  FileTextOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  FieldTimeOutlined,
} from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import type { KpiDashboardDto } from './dashboard-types';

interface KpiStatCardsProps {
  data: KpiDashboardDto;
  loading?: boolean;
}

// KpiStatCards — 4 full-color cards (Nyasha-style) showing primary KPI metrics
export const KpiStatCards = memo(function KpiStatCards({ data, loading }: KpiStatCardsProps) {
  const { t } = useTranslation();

  const cards = useMemo(() => [
    {
      title: t('page.dashboard.kpi.totalCases'),
      value: data.totalCases,
      ariaLabel: t('page.dashboard.kpi.totalCasesAria', { count: data.totalCases }),
      icon: <FileTextOutlined style={{ fontSize: 28 }} />,
      bg: '#007BFF',
      textColor: '#FFFFFF',
    },
    {
      title: t('page.dashboard.kpi.openCases'),
      value: data.openCases,
      ariaLabel: t('page.dashboard.kpi.openCasesAria', { count: data.openCases }),
      icon: <ClockCircleOutlined style={{ fontSize: 28 }} />,
      bg: '#343A40',
      textColor: '#FFFFFF',
    },
    {
      title: t('page.dashboard.kpi.closedCases'),
      value: data.closedCases,
      ariaLabel: t('page.dashboard.kpi.closedCasesAria', { count: data.closedCases }),
      icon: <CheckCircleOutlined style={{ fontSize: 28 }} />,
      bg: '#FFC107',
      textColor: '#212529',
    },
    {
      title: t('page.dashboard.kpi.avgResolution'),
      value: Number(data.averageResolutionDays.toFixed(1)),
      ariaLabel: t('page.dashboard.kpi.avgResolutionAria', { days: data.averageResolutionDays.toFixed(1) }),
      icon: <FieldTimeOutlined style={{ fontSize: 28 }} />,
      bg: '#DC3545',
      textColor: '#FFFFFF',
      precision: 1,
    },
  ], [data, t]);

  return (
    <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
      {cards.map((card) => (
        <Col key={card.title} xs={24} sm={12} xl={6}>
          <Card
            loading={loading}
            variant="borderless"
            aria-label={card.ariaLabel}
            style={{
              background: card.bg,
              borderRadius: 12,
              border: 'none',
              overflow: 'hidden',
            }}
            styles={{ body: { padding: '20px 24px' } }}
          >
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <div>
                <div style={{
                  color: card.textColor,
                  opacity: 0.85,
                  fontSize: 13,
                  fontWeight: 500,
                  marginBottom: 8,
                  whiteSpace: 'nowrap',
                }}>
                  {card.title}
                </div>
                <div style={{
                  color: card.textColor,
                  fontSize: 28,
                  fontWeight: 700,
                  lineHeight: 1.2,
                }}>
                  {card.precision != null
                    ? card.value.toLocaleString(undefined, { minimumFractionDigits: card.precision })
                    : card.value.toLocaleString()}
                </div>
              </div>
              <div style={{ color: card.textColor, opacity: 0.7 }}>
                {card.icon}
              </div>
            </div>
          </Card>
        </Col>
      ))}
    </Row>
  );
});
