import { Card, Table } from 'antd';
import { memo, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import type { ColumnsType } from 'antd/es/table';
import type { AssigneeKpiDto } from './dashboard-types';

interface TopAssigneesTableProps {
  data: AssigneeKpiDto[];
  loading?: boolean;
}

// TopAssigneesTable — shows top assignee performance metrics (memoized columns)
export const TopAssigneesTable = memo(function TopAssigneesTable({ data, loading }: TopAssigneesTableProps) {
  const { t } = useTranslation();

  const columns: ColumnsType<AssigneeKpiDto> = useMemo(() => [
    { title: t('page.dashboard.chart.colAssigneeName'), dataIndex: 'assigneeName', key: 'assigneeName', ellipsis: true },
    { title: t('page.dashboard.chart.colAssigned'), dataIndex: 'totalAssigned', key: 'totalAssigned', width: 90, align: 'right' },
    { title: t('page.dashboard.chart.colClosed'), dataIndex: 'closed', key: 'closed', width: 90, align: 'right' },
    {
      title: t('page.dashboard.chart.colAvgResolution'),
      dataIndex: 'avgResolutionDays',
      key: 'avgResolutionDays',
      width: 160,
      align: 'right',
      render: (v: number) => v.toFixed(1),
    },
  ], [t]);

  return (
    <Card title={t('page.dashboard.chart.topAssignees')} variant="borderless" className="gov-card-hover" style={{ boxShadow: 'var(--elevation-1)' }}>
      <Table<AssigneeKpiDto>
        rowKey="assigneeName"
        columns={columns}
        dataSource={data}
        loading={loading}
        size="small"
        pagination={false}
        locale={{ emptyText: t('common.noData') }}
      />
    </Card>
  );
});
