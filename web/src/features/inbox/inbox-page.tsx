import { Table, Tag, Button, Space, Tooltip } from 'antd';
import { InboxOutlined, ArrowRightOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useWorkflowInbox } from './inbox-api';
import { CaseStatusTag, CasePriorityTag } from '@/features/cases/case-status-tag';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import type { CaseDto, CaseStatus, CasePriority } from '@/features/cases/case-types';

// WorkflowInboxPage — cases pending action by the current user
export function WorkflowInboxPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data, isFetching } = useWorkflowInbox();
  const cases = data?.items ?? [];

  // Action label derived from case status
  function actionLabel(status: CaseStatus): string {
    const key = `page.inbox.actionLabel.${status}`;
    const translated = t(key);
    // Fall back to raw status value if no translation key exists
    return translated === key ? status : translated;
  }

  const COLUMNS: ColumnsType<CaseDto> = [
    { title: t('page.inbox.col.caseNumber'), dataIndex: 'caseNumber', key: 'caseNumber', width: 160 },
    { title: t('page.inbox.col.title'), dataIndex: 'title', key: 'title', ellipsis: true },
    {
      title: t('page.inbox.col.action'),
      dataIndex: 'status',
      key: 'action',
      width: 200,
      render: (v: CaseStatus) => <Tag color="orange">{actionLabel(v)}</Tag>,
    },
    {
      title: t('page.inbox.col.status'),
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: (v: CaseStatus) => <CaseStatusTag status={v} />,
    },
    {
      title: t('page.inbox.col.priority'),
      dataIndex: 'priority',
      key: 'priority',
      width: 110,
      render: (v: CasePriority) => <CasePriorityTag priority={v} />,
    },
    {
      title: t('page.inbox.col.assignedAt'),
      dataIndex: 'assignedAtUtc',
      key: 'assignedAtUtc',
      width: 140,
      render: (v?: string) => v ? dayjs(v).format('DD/MM/YYYY HH:mm') : '—',
    },
    {
      title: t('page.inbox.col.actions'),
      key: 'actions',
      width: 100,
      render: (_, record) => (
        <Space>
          <Tooltip title={t('page.inbox.process', 'Xử lý')}>
            <Button
              size="small"
              type="primary"
              icon={<ArrowRightOutlined />}
              onClick={() => navigate({ to: '/cases/$caseId', params: { caseId: record.id } })}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <AdminPageHeader
        title={t('page.inbox.title')}
        icon={<InboxOutlined />}
        stats={{ total: cases.length, label: t('common.items') }}
      />
      <AdminContentCard noPadding>
        <Table<CaseDto>
          rowKey="id"
          columns={COLUMNS}
          dataSource={cases}
          loading={isFetching}
          size="small"
          scroll={{ x: 900 }}
          locale={{ emptyText: t('page.inbox.empty') }}
          pagination={{ pageSize: 20 }}
        />
      </AdminContentCard>
    </div>
  );
}
