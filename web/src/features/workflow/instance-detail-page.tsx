// Workflow instance detail — metadata, available transitions, history timeline

import { Spin, Descriptions, Button, Space, Typography, message } from 'antd';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/features/auth';
import dayjs from 'dayjs';
import { useWorkflowInstance, useAvailableTransitions, useExecuteTransition } from './workflow-api';
import type { ExecuteTransitionRequest, WorkflowTransitionDto } from './workflow-types';
import { TransitionTimeline } from './components/transition-timeline';

const { Title } = Typography;

interface Props {
  instanceId: string;
}

export function InstanceDetailPage({ instanceId }: Props) {
  const { t } = useTranslation();
  const { user } = useAuth();
  const actorId = user?.profile?.sub ?? '';

  const { data: instance, isLoading: loadingInstance } = useWorkflowInstance(instanceId);
  const { data: transitions = [], isLoading: loadingTransitions } = useAvailableTransitions(instanceId);
  const executeMutation = useExecuteTransition(instanceId);

  const handleTransition = async (tr: WorkflowTransitionDto) => {
    const body: ExecuteTransitionRequest = {
      transitionId: tr.id,
      actorId,
      actorRoleCode: tr.requiredRoleCode ?? null,
    };
    await executeMutation.mutateAsync(body);
    message.success(t('workflow.instance.transitionSuccess'));
  };

  if (loadingInstance) {
    return <Spin size="large" style={{ display: 'block', margin: '80px auto' }} />;
  }

  if (!instance) {
    return <div>{t('common.notFound')}</div>;
  }

  return (
    <div>
      <Title level={4} style={{ marginBottom: 16 }}>
        {t('workflow.instance.detailTitle')}
      </Title>

      {/* Metadata */}
      <Descriptions bordered size="small" column={2} style={{ marginBottom: 24 }}>
        <Descriptions.Item label={t('workflow.instance.entityType')}>
          {instance.entityType}
        </Descriptions.Item>
        <Descriptions.Item label={t('workflow.instance.entityId')}>
          {instance.entityId}
        </Descriptions.Item>
        <Descriptions.Item label={t('workflow.instance.definition')}>
          {instance.definitionId}
        </Descriptions.Item>
        <Descriptions.Item label={t('workflow.instance.currentState')}>
          {instance.currentStateId}
        </Descriptions.Item>
        <Descriptions.Item label={t('workflow.instance.started')}>
          {dayjs(instance.startedAt).format('DD/MM/YYYY HH:mm')}
        </Descriptions.Item>
        <Descriptions.Item label={t('workflow.instance.due')}>
          {instance.dueAt ? dayjs(instance.dueAt).format('DD/MM/YYYY HH:mm') : '—'}
        </Descriptions.Item>
        {instance.completedAt && (
          <Descriptions.Item label={t('workflow.instance.completed')}>
            {dayjs(instance.completedAt).format('DD/MM/YYYY HH:mm')}
          </Descriptions.Item>
        )}
      </Descriptions>

      {/* Available transitions */}
      <Title level={5} style={{ marginBottom: 12 }}>{t('workflow.instance.availableTransitions')}</Title>
      <Space wrap style={{ marginBottom: 24 }}>
        {loadingTransitions ? (
          <Spin size="small" />
        ) : transitions.length === 0 ? (
          <span style={{ color: '#888' }}>{t('workflow.instance.noTransitions')}</span>
        ) : (
          transitions.map((tr) => (
            <Button
              key={tr.id}
              onClick={() => handleTransition(tr)}
              loading={executeMutation.isPending}
              disabled={!actorId}
            >
              {tr.actionLabelEn || tr.actionName}
            </Button>
          ))
        )}
      </Space>

      {/* History timeline */}
      <Title level={5} style={{ marginBottom: 12 }}>{t('workflow.instance.history')}</Title>
      <TransitionTimeline history={instance.history} />
    </div>
  );
}
