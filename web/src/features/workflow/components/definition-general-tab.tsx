// General tab — edit definition metadata (name, description, status info)

import { useState } from 'react';
import { Button, Form, Input, Descriptions, message } from 'antd';
import { useTranslation } from 'react-i18next';
import { useUpdateWorkflowDefinition } from '../workflow-api';
import type { WorkflowDefinitionDto } from '../workflow-types';
import dayjs from 'dayjs';

interface Props {
  definition: WorkflowDefinitionDto;
}

export function DefinitionGeneralTab({ definition }: Props) {
  const { t } = useTranslation();
  const [editing, setEditing] = useState(false);
  const [form] = Form.useForm();
  const updateMutation = useUpdateWorkflowDefinition(definition.id);

  const handleSave = async () => {
    const values = await form.validateFields();
    await updateMutation.mutateAsync(values);
    message.success(t('common.saved'));
    setEditing(false);
  };

  if (editing) {
    return (
      <Form
        form={form}
        layout="vertical"
        initialValues={{ name: definition.name, description: definition.description }}
        style={{ maxWidth: 600 }}
      >
        <Form.Item name="name" label={t('workflow.col.name')} rules={[{ required: true }]}>
          <Input />
        </Form.Item>
        <Form.Item name="description" label={t('workflow.description')}>
          <Input.TextArea rows={4} />
        </Form.Item>
        <Button type="primary" onClick={handleSave} loading={updateMutation.isPending} style={{ marginRight: 8 }}>
          {t('common.save')}
        </Button>
        <Button onClick={() => setEditing(false)}>{t('common.cancel')}</Button>
      </Form>
    );
  }

  return (
    <div>
      <Descriptions column={2} bordered size="small" style={{ marginBottom: 16 }}>
        <Descriptions.Item label={t('workflow.col.name')}>{definition.name}</Descriptions.Item>
        <Descriptions.Item label={t('workflow.definitionKey')}>{definition.definitionKey}</Descriptions.Item>
        <Descriptions.Item label={t('workflow.description')} span={2}>{definition.description || '—'}</Descriptions.Item>
        <Descriptions.Item label={t('workflow.version')}>v{definition.version}</Descriptions.Item>
        <Descriptions.Item label={t('workflow.isLatest')}>{definition.isLatest ? t('common.yes') : t('common.no')}</Descriptions.Item>
        <Descriptions.Item label={t('workflow.col.states')}>{definition.states.length}</Descriptions.Item>
        <Descriptions.Item label={t('workflow.col.transitions')}>{definition.transitions.length}</Descriptions.Item>
        <Descriptions.Item label={t('workflow.col.createdAt')}>{dayjs(definition.createdAt).format('DD/MM/YYYY HH:mm')}</Descriptions.Item>
      </Descriptions>
      <Button onClick={() => setEditing(true)}>{t('common.edit')}</Button>
    </div>
  );
}
