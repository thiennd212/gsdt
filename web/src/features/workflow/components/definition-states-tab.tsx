// States tab — CRUD table for workflow states with inline color badges

import { useState } from 'react';
import { Table, Button, Space, Tag, Popconfirm, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useAuth } from '@/features/auth';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import { useSaveDefinitionGraph } from '../workflow-api';
import type { WorkflowDefinitionDto, WorkflowStateDto } from '../workflow-types';
import { StateEditorModal } from './state-editor-modal';
import { statesToGraphInputs, transitionsToGraphInputs } from './graph-converters';

interface Props {
  definition: WorkflowDefinitionDto;
}

export function DefinitionStatesTab({ definition }: Props) {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [modalOpen, setModalOpen] = useState(false);
  const [editingState, setEditingState] = useState<WorkflowStateDto | null>(null);
  const saveMutation = useSaveDefinitionGraph(definition.id);

  const handleSave = async (values: Omit<WorkflowStateDto, 'id'> & { id?: string }) => {
    const currentStates = [...definition.states];
    if (values.id) {
      const idx = currentStates.findIndex((s) => s.id === values.id);
      if (idx >= 0) currentStates[idx] = { ...currentStates[idx], ...values, id: values.id };
    } else {
      currentStates.push({ ...values, id: crypto.randomUUID() } as WorkflowStateDto);
    }
    await saveMutation.mutateAsync({
      tenantId: definition.tenantId,
      modifiedBy: user?.profile?.sub ?? '',
      states: statesToGraphInputs(currentStates),
      transitions: transitionsToGraphInputs(definition.transitions, currentStates),
    });
    message.success(t('common.saved'));
    setModalOpen(false);
    setEditingState(null);
  };

  const handleDelete = async (stateId: string) => {
    const filtered = definition.states.filter((s) => s.id !== stateId);
    const filteredTransitions = definition.transitions.filter(
      (tr) => tr.fromStateId !== stateId && tr.toStateId !== stateId,
    );
    await saveMutation.mutateAsync({
      tenantId: definition.tenantId,
      modifiedBy: user?.profile?.sub ?? '',
      states: statesToGraphInputs(filtered),
      transitions: transitionsToGraphInputs(filteredTransitions, filtered),
    });
    message.success(t('common.deleted'));
  };

  const columns: ColumnsType<WorkflowStateDto> = [
    {
      title: t('workflow.color'),
      dataIndex: 'color',
      width: 50,
      render: (c: string) => <div style={{ width: 20, height: 20, borderRadius: 4, background: c }} />,
    },
    { title: t('workflow.stateName'), dataIndex: 'name', width: 140 },
    { title: t('workflow.displayNameVi'), dataIndex: 'displayNameVi' },
    { title: t('workflow.displayNameEn'), dataIndex: 'displayNameEn' },
    {
      title: t('workflow.type'),
      width: 120,
      render: (_: unknown, r: WorkflowStateDto) => (
        <Space>
          {r.isInitial && <Tag color="green">{t('workflow.initial')}</Tag>}
          {r.isFinal && <Tag color="red">{t('workflow.final')}</Tag>}
        </Space>
      ),
    },
    { title: t('workflow.sortOrder'), dataIndex: 'sortOrder', width: 80 },
    {
      title: t('common.actions'),
      width: 100,
      render: (_: unknown, r: WorkflowStateDto) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => { setEditingState(r); setModalOpen(true); }} />
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
        onClick={() => { setEditingState(null); setModalOpen(true); }}
        style={{ marginBottom: 12 }}
      >
        {t('workflow.addState')}
      </Button>
      <Table<WorkflowStateDto>
        rowKey="id"
        columns={columns}
        dataSource={definition.states}
        size="small"
        pagination={false}
        loading={saveMutation.isPending}
      />
      <StateEditorModal
        open={modalOpen}
        state={editingState}
        onSave={handleSave}
        onCancel={() => { setModalOpen(false); setEditingState(null); }}
      />
    </div>
  );
}
