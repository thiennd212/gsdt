// Workflow definition detail page — 7-tab layout for managing a single definition

import { Spin, Tabs, Typography, Space, Tag, Button, Popconfirm, message } from 'antd';
import {
  ArrowLeftOutlined,
  CopyOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import { useNavigate } from '@tanstack/react-router';
import {
  useWorkflowDefinition,
  useCloneWorkflowDefinition,
  useActivateDefinition,
  useDeactivateDefinition,
} from './workflow-api';
import { DefinitionGeneralTab } from './components/definition-general-tab';
import { DefinitionStatesTab } from './components/definition-states-tab';
import { DefinitionTransitionsTab } from './components/definition-transitions-tab';
import { DefinitionVisualDesignerTab } from './components/definition-visual-designer-tab';
import { DefinitionNotificationsTab } from './components/definition-notifications-tab';
import { DefinitionAssignmentRulesTab } from './components/definition-assignment-rules-tab';

const { Title } = Typography;

interface Props {
  definitionId: string;
}

export function WorkflowDefinitionDetailPage({ definitionId }: Props) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data: definition, isLoading } = useWorkflowDefinition(definitionId);
  const cloneMutation = useCloneWorkflowDefinition(definitionId);
  const activateMutation = useActivateDefinition(definitionId);
  const deactivateMutation = useDeactivateDefinition(definitionId);

  if (isLoading) return <Spin size="large" style={{ display: 'block', margin: '80px auto' }} />;
  if (!definition) return <div>{t('common.notFound')}</div>;

  const handleClone = async () => {
    const cloned = await cloneMutation.mutateAsync();
    message.success(t('workflow.cloneSuccess'));
    navigate({ to: '/admin/workflow/$definitionId', params: { definitionId: cloned.id } });
  };

  const handleToggleActive = async () => {
    if (definition.isActive) {
      await deactivateMutation.mutateAsync();
      message.success(t('workflow.deactivated'));
    } else {
      await activateMutation.mutateAsync();
      message.success(t('workflow.activated'));
    }
  };

  const tabItems = [
    { key: 'general', label: t('workflow.tabs.general'), children: <DefinitionGeneralTab definition={definition} /> },
    { key: 'designer', label: t('workflow.tabs.designer'), children: <DefinitionVisualDesignerTab definition={definition} /> },
    { key: 'states', label: `${t('workflow.tabs.states')} (${definition.states.length})`, children: <DefinitionStatesTab definition={definition} /> },
    { key: 'transitions', label: `${t('workflow.tabs.transitions')} (${definition.transitions.length})`, children: <DefinitionTransitionsTab definition={definition} /> },
    { key: 'notifications', label: t('workflow.tabs.notifications'), children: <DefinitionNotificationsTab definitionId={definitionId} /> },
    { key: 'assignments', label: t('workflow.tabs.assignments'), children: <DefinitionAssignmentRulesTab /> },
  ];

  return (
    <div>
      <Space style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between' }}>
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate({ to: '/admin/workflow' })} />
          <Title level={4} style={{ margin: 0 }}>{definition.name}</Title>
          <Tag color={definition.isActive ? 'green' : 'default'}>
            {definition.isActive ? t('workflow.active') : t('workflow.inactive')}
          </Tag>
          <Tag>v{definition.version}</Tag>
        </Space>
        <Space>
          <Popconfirm title={t('workflow.cloneConfirm')} onConfirm={handleClone}>
            <Button icon={<CopyOutlined />} loading={cloneMutation.isPending}>
              {t('workflow.clone')}
            </Button>
          </Popconfirm>
          <Popconfirm
            title={definition.isActive ? t('workflow.deactivateConfirm') : t('workflow.activateConfirm')}
            onConfirm={handleToggleActive}
          >
            <Button
              icon={definition.isActive ? <StopOutlined /> : <CheckCircleOutlined />}
              loading={activateMutation.isPending || deactivateMutation.isPending}
              type={definition.isActive ? 'default' : 'primary'}
            >
              {definition.isActive ? t('workflow.deactivate') : t('workflow.activate')}
            </Button>
          </Popconfirm>
        </Space>
      </Space>

      <Tabs items={tabItems} defaultActiveKey="general" destroyInactiveTabPane={false} />
    </div>
  );
}
