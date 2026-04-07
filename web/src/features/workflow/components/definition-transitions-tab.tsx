// Transitions tab — CRUD table for workflow transitions

import { useState } from 'react';
import { Table, Button, Space, Tag, Popconfirm, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useAuth } from '@/features/auth';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import { useSaveDefinitionGraph } from '../workflow-api';
import type { WorkflowDefinitionDto, WorkflowTransitionDto } from '../workflow-types';
import { TransitionEditorModal } from './transition-editor-modal';
import { statesToGraphInputs, transitionsToGraphInputs } from './graph-converters';

interface Props {
  definition: WorkflowDefinitionDto;
}

export function DefinitionTransitionsTab({ definition }: Props) {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [modalOpen, setModalOpen] = useState(false);
  const [editingTransition, setEditingTransition] = useState<WorkflowTransitionDto | null>(null);
  const saveMutation = useSaveDefinitionGraph(definition.id);

  const stateMap = new Map(definition.states.map((s) => [s.id, s]));

  const handleSave = async (values: Omit<WorkflowTransitionDto, 'id'> & { id?: string }) => {
    const currentTransitions = [...definition.transitions];
    if (values.id) {
      const idx = currentTransitions.findIndex((tr) => tr.id === values.id);
      if (idx >= 0) currentTransitions[idx] = { ...currentTransitions[idx], ...values, id: values.id };
    } else {
      currentTransitions.push({ ...values, id: crypto.randomUUID() } as WorkflowTransitionDto);
    }
    await saveMutation.mutateAsync({
      tenantId: definition.tenantId,
      modifiedBy: user?.profile?.sub ?? '',
      states: statesToGraphInputs(definition.states),
      transitions: transitionsToGraphInputs(currentTransitions, definition.states),
    });
    message.success(t('common.saved'));
    setModalOpen(false);
    setEditingTransition(null);
  };

  const handleDelete = async (transitionId: string) => {
    const filtered = definition.transitions.filter((tr) => tr.id !== transitionId);
    await saveMutation.mutateAsync({
      tenantId: definition.tenantId,
      modifiedBy: user?.profile?.sub ?? '',
      states: statesToGraphInputs(definition.states),
      transitions: transitionsToGraphInputs(filtered, definition.states),
    });
    message.success(t('common.deleted'));
  };

  const columns: ColumnsType<WorkflowTransitionDto> = [
    {
      title: t('workflow.fromState'),
      dataIndex: 'fromStateId',
      render: (id: string) => {
        const s = stateMap.get(id);
        return s ? <Tag color={s.color}>{s.displayNameVi}</Tag> : id;
      },
    },
    {
      title: t('workflow.toState'),
      dataIndex: 'toStateId',
      render: (id: string) => {
        const s = stateMap.get(id);
        return s ? <Tag color={s.color}>{s.displayNameVi}</Tag> : id;
      },
    },
    { title: t('workflow.actionName'), dataIndex: 'actionName', width: 140 },
    { title: t('workflow.actionLabelVi'), dataIndex: 'actionLabelVi' },
    {
      title: t('workflow.requiredRole'),
      dataIndex: 'requiredRoleCode',
      width: 120,
      render: (v: string | null) => v || <span style={{ color: '#aaa' }}>—</span>,
    },
    {
      title: t('common.actions'),
      width: 100,
      render: (_: unknown, r: WorkflowTransitionDto) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => { setEditingTransition(r); setModalOpen(true); }} />
          <Popconfirm title={t('common.confirmDelete')} onConfirm={() => handleDelete(r.id)}>
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Button
        type="primary"
        icon={<PlusOutlined />}
        onClick={() => { setEditingTransition(null); setModalOpen(true); }}
        style={{ marginBottom: 12 }}
        disabled={definition.states.length < 2}
      >
        {t('workflow.addTransition')}
      </Button>
      <Table<WorkflowTransitionDto>
        rowKey="id"
        columns={columns}
        dataSource={definition.transitions}
        size="small"
        pagination={false}
        loading={saveMutation.isPending}
      />
      <TransitionEditorModal
        open={modalOpen}
        transition={editingTransition}
        states={definition.states}
        onSave={handleSave}
        onCancel={() => { setModalOpen(false); setEditingTransition(null); }}
      />
    </div>
  );
}
