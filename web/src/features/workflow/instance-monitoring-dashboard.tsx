// Instance monitoring dashboard — KPI cards + instances table for admins

import { Table, Typography, Space, Tag, Row, Col, Card, Statistic } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import type { WorkflowInstanceDto } from './workflow-types';

const { Title } = Typography;

/** Derive status from instance: completed if completedAt set, else active */
function instanceStatus(r: WorkflowInstanceDto): 'Active' | 'Completed' {
  return r.completedAt ? 'Completed' : 'Active';
}

interface Props {
  instances?: WorkflowInstanceDto[];
  isLoading?: boolean;
}

export function InstanceMonitoringDashboard({ instances = [], isLoading = false }: Props) {
  const { t } = useTranslation();

  // KPI derivations from local data
  const active = instances.filter((i) => !i.completedAt).length;
  const today = dayjs().format('YYYY-MM-DD');
  const completedToday = instances.filter(
    (i) => i.completedAt && i.completedAt.startsWith(today),
  ).length;
  const breached = instances.filter(
    (i) => !i.completedAt && i.dueAt && dayjs(i.dueAt).isBefore(dayjs()),
  ).length;
  const slaBreachRate = instances.length > 0 ? Math.round((breached / instances.length) * 100) : 0;

  const columns: ColumnsType<WorkflowInstanceDto> = [
    { title: t('workflow.instance.entityType'), dataIndex: 'entityType', ellipsis: true },
    { title: t('workflow.instance.entityId'), dataIndex: 'entityId', width: 160, ellipsis: true },
    { title: t('workflow.instance.currentState'), dataIndex: 'currentStateId', width: 140, ellipsis: true },
    {
      title: t('workflow.instance.started'),
      dataIndex: 'startedAt',
      width: 110,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
    {
      title: t('workflow.instance.due'),
      dataIndex: 'dueAt',
      width: 110,
      render: (v: string | null) => (v ? dayjs(v).format('DD/MM/YYYY') : '—'),
    },
    {
      title: t('workflow.instance.status'),
      width: 100,
      render: (_: unknown, r: WorkflowInstanceDto) => {
        const s = instanceStatus(r);
        return <Tag color={s === 'Active' ? 'blue' : 'green'}>{s}</Tag>;
      },
    },
  ];

  return (
    <div>
      <Title level={4} style={{ marginBottom: 16 }}>{t('workflow.instance.dashboardTitle')}</Title>

      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card size="small">
            <Statistic title={t('workflow.instance.kpiActive')} value={active} />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic title={t('workflow.instance.kpiCompletedToday')} value={completedToday} />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title={t('workflow.instance.kpiSlaBreachRate')}
              value={slaBreachRate}
              suffix="%"
              valueStyle={slaBreachRate > 10 ? { color: '#cf1322' } : undefined}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic title={t('workflow.instance.kpiTotal')} value={instances.length} />
          </Card>
        </Col>
      </Row>

      <Space style={{ marginBottom: 12 }}>
        <Title level={5} style={{ margin: 0 }}>{t('workflow.instance.tableTitle')}</Title>
      </Space>

      <Table<WorkflowInstanceDto>
        rowKey="id"
        columns={columns}
        dataSource={instances}
        loading={isLoading}
        size="small"
        pagination={{ pageSize: 20, showSizeChanger: false }}
      />
    </div>
  );
}
