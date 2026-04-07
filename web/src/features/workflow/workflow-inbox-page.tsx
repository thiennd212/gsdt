// Generic workflow inbox — tasks assigned to current user with filter + drawer

import { useState, useMemo } from 'react';
import { Table, Space, Tag, Select } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useAuth } from '@/features/auth';
import { useTasksByAssignee } from './workflow-api';
import type { WorkflowTaskDto } from './workflow-types';
import { TaskDetailDrawer } from './components/task-detail-drawer';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';

const STATUS_LABELS: Record<number, string> = { 0: 'Open', 1: 'InProgress', 2: 'Done', 3: 'Cancelled' };
const STATUS_COLORS: Record<number, string> = { 0: 'blue', 1: 'orange', 2: 'green', 3: 'default' };
const PRIORITY_LABELS: Record<number, string> = { 0: 'Low', 1: 'Normal', 2: 'High', 3: 'Urgent' };
const PRIORITY_COLORS: Record<number, string> = { 0: 'default', 1: 'blue', 2: 'orange', 3: 'red' };

export function WorkflowInboxPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const userId = user?.profile?.sub ?? '';

  const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined);
  const [priorityFilter, setPriorityFilter] = useState<number | undefined>(undefined);
  const [selectedTask, setSelectedTask] = useState<WorkflowTaskDto | null>(null);

  const { data: tasks = [], isLoading } = useTasksByAssignee(userId);

  const filtered = useMemo(() => tasks.filter((t) => {
    if (statusFilter !== undefined && t.status !== statusFilter) return false;
    if (priorityFilter !== undefined && t.priority !== priorityFilter) return false;
    return true;
  }), [tasks, statusFilter, priorityFilter]);

  const columns: ColumnsType<WorkflowTaskDto> = [
    {
      title: t('workflow.inbox.title'),
      dataIndex: 'title',
      ellipsis: true,
      render: (v: string, r) => <a onClick={() => setSelectedTask(r)}>{v}</a>,
    },
    {
      title: t('workflow.inbox.status'),
      width: 110,
      render: (_: unknown, r: WorkflowTaskDto) => (
        <Tag color={STATUS_COLORS[r.status]}>{STATUS_LABELS[r.status]}</Tag>
      ),
    },
    {
      title: t('workflow.inbox.priority'),
      width: 100,
      render: (_: unknown, r: WorkflowTaskDto) => (
        <Tag color={PRIORITY_COLORS[r.priority]}>{PRIORITY_LABELS[r.priority]}</Tag>
      ),
    },
    {
      title: t('workflow.inbox.dueDate'),
      dataIndex: 'dueDate',
      width: 110,
      render: (v: string | null) => (v ? dayjs(v).format('DD/MM/YYYY') : '—'),
    },
    {
      title: t('workflow.inbox.created'),
      dataIndex: 'createdAt',
      width: 110,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
  ];

  return (
    <>
      <AdminPageHeader
        title={t('workflow.inbox.pageTitle')}
        actions={
          <Space>
            <Select
              allowClear
              placeholder={t('workflow.inbox.filterStatus')}
              style={{ width: 140 }}
              onChange={setStatusFilter}
              options={Object.entries(STATUS_LABELS).map(([k, v]) => ({ value: Number(k), label: v }))}
            />
            <Select
              allowClear
              placeholder={t('workflow.inbox.filterPriority')}
              style={{ width: 140 }}
              onChange={setPriorityFilter}
              options={Object.entries(PRIORITY_LABELS).map(([k, v]) => ({ value: Number(k), label: v }))}
            />
          </Space>
        }
      />
      <AdminContentCard noPadding>
        <Table<WorkflowTaskDto>
          rowKey="id"
          columns={columns}
          dataSource={filtered}
          loading={isLoading}
          size="small"
          pagination={{ pageSize: 20, showSizeChanger: false }}
          onRow={(r) => ({ onClick: () => setSelectedTask(r) })}
        />
      </AdminContentCard>

      <TaskDetailDrawer
        task={selectedTask}
        open={!!selectedTask}
        onClose={() => setSelectedTask(null)}
      />
    </>
  );
}
