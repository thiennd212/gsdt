// Task detail drawer — shows task info with complete + reassign actions

import { useState } from 'react';
import { Drawer, Descriptions, Tag, Button, Space, Form, Input, message } from 'antd';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/features/auth';
import { useCompleteTask, useReassignTask } from '../workflow-api';
import type { WorkflowTaskDto, ReassignTaskRequest } from '../workflow-types';

const STATUS_COLORS: Record<number, string> = { 0: 'blue', 1: 'orange', 2: 'green', 3: 'default' };
const STATUS_LABELS: Record<number, string> = { 0: 'Open', 1: 'InProgress', 2: 'Done', 3: 'Cancelled' };
const PRIORITY_COLORS: Record<number, string> = { 0: 'default', 1: 'blue', 2: 'orange', 3: 'red' };
const PRIORITY_LABELS: Record<number, string> = { 0: 'Low', 1: 'Normal', 2: 'High', 3: 'Urgent' };

interface Props {
  task: WorkflowTaskDto | null;
  open: boolean;
  onClose: () => void;
}

export function TaskDetailDrawer({ task, open, onClose }: Props) {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [reassigning, setReassigning] = useState(false);
  const [form] = Form.useForm<{ newAssigneeId: string; reason?: string }>();
  const completeMutation = useCompleteTask();
  const reassignMutation = useReassignTask();

  const handleComplete = async () => {
    if (!task) return;
    await completeMutation.mutateAsync(task.id);
    message.success(t('workflow.inbox.completed'));
    onClose();
  };

  const handleReassign = async () => {
    if (!task) return;
    const values = await form.validateFields();
    const body: ReassignTaskRequest = {
      newAssigneeId: values.newAssigneeId,
      reason: values.reason,
      tenantId: task.tenantId,
    };
    await reassignMutation.mutateAsync({ taskId: task.id, body });
    message.success(t('workflow.inbox.reassigned'));
    form.resetFields();
    setReassigning(false);
    onClose();
  };

  const isActionable = task?.status === 0 || task?.status === 1;
  const actorId = user?.profile?.sub ?? '';

  return (
    <Drawer
      title={task?.title ?? ''}
      open={open}
      onClose={() => { setReassigning(false); form.resetFields(); onClose(); }}
      width={520}
      footer={
        isActionable && (
          <Space>
            <Button
              type="primary"
              loading={completeMutation.isPending}
              onClick={handleComplete}
              disabled={!actorId}
            >
              {t('workflow.inbox.complete')}
            </Button>
            <Button onClick={() => setReassigning(!reassigning)}>
              {t('workflow.inbox.reassign')}
            </Button>
          </Space>
        )
      }
    >
      {task && (
        <>
          <Descriptions column={1} size="small" bordered>
            <Descriptions.Item label={t('workflow.inbox.status')}>
              <Tag color={STATUS_COLORS[task.status]}>{STATUS_LABELS[task.status]}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label={t('workflow.inbox.priority')}>
              <Tag color={PRIORITY_COLORS[task.priority]}>{PRIORITY_LABELS[task.priority]}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label={t('workflow.inbox.assignee')}>{task.assigneeId ?? '—'}</Descriptions.Item>
            <Descriptions.Item label={t('workflow.inbox.dueDate')}>{task.dueDate ?? '—'}</Descriptions.Item>
            <Descriptions.Item label={t('workflow.inbox.created')}>{task.createdAt}</Descriptions.Item>
            {task.description && (
              <Descriptions.Item label={t('workflow.inbox.description')}>{task.description}</Descriptions.Item>
            )}
          </Descriptions>

          {reassigning && (
            <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
              <Form.Item name="newAssigneeId" label={t('workflow.inbox.newAssignee')} rules={[{ required: true }]}>
                <Input />
              </Form.Item>
              <Form.Item name="reason" label={t('workflow.inbox.reason')}>
                <Input.TextArea rows={2} />
              </Form.Item>
              <Button type="primary" loading={reassignMutation.isPending} onClick={handleReassign}>
                {t('common.save')}
              </Button>
            </Form>
          )}
        </>
      )}
    </Drawer>
  );
}
