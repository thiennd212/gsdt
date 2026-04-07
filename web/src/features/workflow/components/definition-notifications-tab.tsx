// Notifications tab — manage notification configs per definition (action+channel)

import { useState } from 'react';
import { Table, Button, Tag, Popconfirm, Modal, Form, Input, Select, message } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useTranslation } from 'react-i18next';
import {
  useNotificationConfigs,
  useCreateNotificationConfig,
  useDeleteNotificationConfig,
} from '../workflow-api';
import type { WorkflowNotificationConfigDto } from '../workflow-types';

const CHANNELS = ['email', 'sms', 'inapp'];
const RECIPIENT_TYPES = ['role', 'user', 'creator'];

interface Props {
  definitionId: string;
}

export function DefinitionNotificationsTab({ definitionId }: Props) {
  const { t } = useTranslation();
  const [modalOpen, setModalOpen] = useState(false);
  const [form] = Form.useForm();
  const { data: configs, isLoading } = useNotificationConfigs(definitionId);
  const createMutation = useCreateNotificationConfig(definitionId);
  const deleteMutation = useDeleteNotificationConfig(definitionId);

  const handleCreate = async () => {
    const values = await form.validateFields();
    await createMutation.mutateAsync(values);
    message.success(t('common.saved'));
    setModalOpen(false);
    form.resetFields();
  };

  const channelColor: Record<string, string> = { email: 'blue', sms: 'green', inapp: 'orange' };

  const columns: ColumnsType<WorkflowNotificationConfigDto> = [
    { title: t('workflow.actionName'), dataIndex: 'actionName', width: 140 },
    {
      title: t('workflow.notifChannel'),
      dataIndex: 'channel',
      width: 90,
      render: (v: string) => <Tag color={channelColor[v] ?? 'default'}>{v}</Tag>,
    },
    { title: t('workflow.recipientType'), dataIndex: 'recipientType', width: 100 },
    { title: t('workflow.recipientValue'), dataIndex: 'recipientValue', ellipsis: true },
    { title: t('workflow.notifSubject'), dataIndex: 'subject', ellipsis: true },
    {
      title: t('common.actions'),
      width: 60,
      render: (_: unknown, r: WorkflowNotificationConfigDto) => (
        <Popconfirm title={t('common.confirmDelete')} onConfirm={() => deleteMutation.mutateAsync(r.id)}>
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  return (
    <div>
      <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)} style={{ marginBottom: 12 }}>
        {t('workflow.addNotification')}
      </Button>
      <Table<WorkflowNotificationConfigDto>
        rowKey="id"
        columns={columns}
        dataSource={configs ?? []}
        size="small"
        pagination={false}
        loading={isLoading}
      />
      <Modal
        title={t('workflow.addNotification')}
        open={modalOpen}
        onOk={handleCreate}
        onCancel={() => { setModalOpen(false); form.resetFields(); }}
        confirmLoading={createMutation.isPending}
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        destroyOnHidden
      >
        <Form form={form} layout="vertical">
          <Form.Item name="actionName" label={t('workflow.actionName')} rules={[{ required: true }]}>
            <Input placeholder="e.g. Submit, Approve" />
          </Form.Item>
          <div style={{ display: 'flex', gap: 16 }}>
            <Form.Item name="channel" label={t('workflow.notifChannel')} rules={[{ required: true }]} style={{ flex: 1 }}>
              <Select options={CHANNELS.map((c) => ({ label: c, value: c }))} />
            </Form.Item>
            <Form.Item name="recipientType" label={t('workflow.recipientType')} rules={[{ required: true }]} style={{ flex: 1 }}>
              <Select options={RECIPIENT_TYPES.map((r) => ({ label: r, value: r }))} />
            </Form.Item>
          </div>
          <Form.Item name="recipientValue" label={t('workflow.recipientValue')} rules={[{ required: true }]}>
            <Input placeholder={t('workflow.recipientValuePlaceholder')} />
          </Form.Item>
          <Form.Item name="subject" label={t('workflow.notifSubject')} rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="bodyTemplate" label={t('workflow.notifBody')} rules={[{ required: true }]}>
            <Input.TextArea rows={4} placeholder="Liquid template: {{ instance.entityType }}" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
