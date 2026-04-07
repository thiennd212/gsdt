// Assignment rules tab — shows rules assigned to all definitions (shared with standalone page)

import { Table, Tag, Button, Popconfirm, message } from 'antd';
import { DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import { useAssignmentRules, useDeleteAssignmentRule } from '../workflow-api';
import type { WorkflowAssignmentRuleDto } from '../workflow-types';
import dayjs from 'dayjs';

export function DefinitionAssignmentRulesTab() {
  const { t } = useTranslation();
  const { data: rules, isLoading } = useAssignmentRules();
  const deleteMutation = useDeleteAssignmentRule();

  const handleDelete = async (id: string) => {
    await deleteMutation.mutateAsync(id);
    message.success(t('common.deleted'));
  };

  const columns: ColumnsType<WorkflowAssignmentRuleDto> = [
    { title: t('workflow.entityType'), dataIndex: 'entityType', render: (v: string | null) => v || '*' },
    { title: t('workflow.entitySubType'), dataIndex: 'entitySubType', render: (v: string | null) => v || '*' },
    {
      title: t('workflow.specificity'),
      dataIndex: 'specificity',
      width: 100,
      render: (v: number) => <Tag color={v >= 3 ? 'green' : v >= 2 ? 'blue' : 'default'}>{v}</Tag>,
    },
    { title: t('workflow.priority'), dataIndex: 'priority', width: 80 },
    { title: t('workflow.description'), dataIndex: 'description', ellipsis: true },
    {
      title: t('workflow.effectiveFrom'),
      dataIndex: 'effectiveFrom',
      width: 120,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
    {
      title: t('common.actions'),
      width: 60,
      render: (_: unknown, r: WorkflowAssignmentRuleDto) => (
        <Popconfirm title={t('common.confirmDelete')} onConfirm={() => handleDelete(r.id)}>
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  return (
    <Table<WorkflowAssignmentRuleDto>
      rowKey="id"
      columns={columns}
      dataSource={rules ?? []}
      size="small"
      pagination={false}
      loading={isLoading}
    />
  );
}
