import ReactECharts from 'echarts-for-react';
import { Card } from 'antd';
import { memo, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { GOV_COLORS } from '@/app/theme';
import type { MonthlyTrendDto } from './dashboard-types';

interface MonthlyTrendChartProps {
  data: MonthlyTrendDto[];
}

// Line chart — monthly new vs closed cases trend (memoized to prevent ECharts re-init)
export const MonthlyTrendChart = memo(function MonthlyTrendChart({ data }: MonthlyTrendChartProps) {
  const { t } = useTranslation();

  const option = useMemo(() => {
    const sorted = [...data].sort((a, b) =>
      a.year !== b.year ? a.year - b.year : a.month - b.month,
    );
    const labels = sorted.map((d) => `${String(d.month).padStart(2, '0')}/${d.year}`);
    const newSeries = sorted.map((d) => d.newCases);
    const closedSeries = sorted.map((d) => d.closedCases);
    const newCasesLabel = t('page.dashboard.chart.newCases');
    const closedCasesLabel = t('page.dashboard.chart.closedCases');

    return {
      tooltip: { trigger: 'axis' },
      legend: { data: [newCasesLabel, closedCasesLabel], bottom: 0 },
      grid: { left: 8, right: 16, top: 16, bottom: 36, containLabel: true },
      xAxis: {
        type: 'category',
        data: labels.length > 0 ? labels : ['—'],
        axisLabel: { rotate: 30 },
        boundaryGap: false,
      },
      yAxis: { type: 'value', minInterval: 1, name: t('page.dashboard.chart.countAxis') },
      color: [GOV_COLORS.navy, '#52c41a'],
      series: [
        {
          name: newCasesLabel,
          type: 'line',
          smooth: true,
          data: newSeries,
          areaStyle: { opacity: 0.08 },
          symbol: 'circle',
          symbolSize: 6,
        },
        {
          name: closedCasesLabel,
          type: 'line',
          smooth: true,
          data: closedSeries,
          areaStyle: { opacity: 0.08 },
          symbol: 'circle',
          symbolSize: 6,
        },
      ],
    };
  }, [data, t]);

  return (
    <Card title={t('page.dashboard.chart.monthlyTrend')} variant="borderless" className="gov-card-hover" style={{ boxShadow: 'var(--elevation-1)' }}>
      <ReactECharts option={option} style={{ height: 280 }} opts={{ renderer: 'svg' }} />
    </Card>
  );
});
