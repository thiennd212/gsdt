import ReactECharts from 'echarts-for-react';
import { Card } from 'antd';
import { memo, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { GOV_COLORS } from '@/app/theme';

interface CasesByStatusChartProps {
  data: Record<string, number>;
}

// Donut chart — cases distribution by status (memoized to prevent ECharts re-init)
export const CasesByStatusChart = memo(function CasesByStatusChart({ data }: CasesByStatusChartProps) {
  const { t } = useTranslation();

  const option = useMemo(() => {
    const series = Object.entries(data).map(([name, value]) => ({ name, value }));
    return {
      tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
      legend: { bottom: 0, type: 'scroll' },
      color: [GOV_COLORS.navy, GOV_COLORS.gold, GOV_COLORS.success, GOV_COLORS.error, GOV_COLORS.navyLight],
      series: [
        {
          type: 'pie',
          radius: ['42%', '68%'],
          avoidLabelOverlap: false,
          itemStyle: { borderRadius: 4, borderColor: '#fff', borderWidth: 2 },
          label: { show: false },
          emphasis: { label: { show: true, fontSize: 14, fontWeight: 700 } },
          data: series.length > 0 ? series : [{ name: t('page.dashboard.chart.noData'), value: 1 }],
        },
      ],
    };
  }, [data, t]);

  return (
    <Card title={t('page.dashboard.chart.casesByStatus')} variant="borderless" className="gov-card-hover" style={{ boxShadow: 'var(--elevation-1)' }}>
      <ReactECharts option={option} style={{ height: 280 }} opts={{ renderer: 'svg' }} />
    </Card>
  );
});
