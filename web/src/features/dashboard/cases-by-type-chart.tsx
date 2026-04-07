import ReactECharts from 'echarts-for-react';
import { Card } from 'antd';
import { memo, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { GOV_COLORS } from '@/app/theme';

interface CasesByTypeChartProps {
  data: Record<string, number>;
}

// Horizontal bar chart — cases distribution by type (memoized to prevent ECharts re-init)
export const CasesByTypeChart = memo(function CasesByTypeChart({ data }: CasesByTypeChartProps) {
  const { t } = useTranslation();

  const option = useMemo(() => {
    const entries = Object.entries(data);
    const categories = entries.map(([k]) => k);
    const values = entries.map(([, v]) => v);
    return {
      tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
      grid: { left: 16, right: 24, top: 8, bottom: 8, containLabel: true },
      xAxis: { type: 'value', minInterval: 1 },
      yAxis: {
        type: 'category',
        data: categories.length > 0 ? categories : [t('page.dashboard.chart.noData')],
        axisLabel: { width: 120, overflow: 'truncate' },
      },
      color: [GOV_COLORS.navy],
      series: [
        {
          type: 'bar',
          data: values.length > 0 ? values : [0],
          barMaxWidth: 32,
          itemStyle: { borderRadius: [0, 4, 4, 0] },
          label: { show: true, position: 'right', color: GOV_COLORS.textPrimary },
        },
      ],
    };
  }, [data, t]);

  return (
    <Card title={t('page.dashboard.chart.casesByType')} variant="borderless" className="gov-card-hover" style={{ boxShadow: 'var(--elevation-1)' }}>
      <ReactECharts option={option} style={{ height: 280 }} opts={{ renderer: 'svg' }} />
    </Card>
  );
});
