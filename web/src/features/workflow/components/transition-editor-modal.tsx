// Modal for creating/editing a workflow transition

import { Modal, Form, Input, Select } from 'antd';
import { useTranslation } from 'react-i18next';
import type { WorkflowStateDto, WorkflowTransitionDto } from '../workflow-types';

interface Props {
  open: boolean;
  transition?: WorkflowTransitionDto | null;
  states: WorkflowStateDto[];
  onSave: (values: Omit<WorkflowTransitionDto, 'id'> & { id?: string }) => void;
  onCancel: () => void;
}

export function TransitionEditorModal({ open, transition, states, onSave, onCancel }: Props) {
  const { t } = useTranslation();
  const [form] = Form.useForm();

  const handleOk = async () => {
    const values = await form.validateFields();
    onSave({ ...values, id: transition?.id });
    form.resetFields();
  };

  const stateOptions = states.map((s) => ({ label: s.displayNameVi || s.name, value: s.id }));

  return (
    <Modal
      title={transition ? t('workflow.editTransition') : t('workflow.addTransition')}
      open={open}
      onOk={handleOk}
      onCancel={() => { onCancel(); form.resetFields(); }}
      okText={t('common.save')}
      cancelText={t('common.cancel')}
      destroyOnHidden
      width={560}
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={transition ?? { sortOrder: 0 }}
      >
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item name="fromStateId" label={t('workflow.fromState')} rules={[{ required: true }]} style={{ flex: 1 }}>
            <Select options={stateOptions} placeholder={t('workflow.selectState')} />
          </Form.Item>
          <Form.Item name="toStateId" label={t('workflow.toState')} rules={[{ required: true }]} style={{ flex: 1 }}>
            <Select options={stateOptions} placeholder={t('workflow.selectState')} />
          </Form.Item>
        </div>
        <Form.Item name="actionName" label={t('workflow.actionName')} rules={[{ required: true }]}>
          <Input placeholder="e.g. Submit, Approve, Reject" />
        </Form.Item>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item name="actionLabelVi" label={t('workflow.actionLabelVi')} rules={[{ required: true }]} style={{ flex: 1 }}>
            <Input placeholder="Nhãn tiếng Việt" />
          </Form.Item>
          <Form.Item name="actionLabelEn" label={t('workflow.actionLabelEn')} rules={[{ required: true }]} style={{ flex: 1 }}>
            <Input placeholder="English label" />
          </Form.Item>
        </div>
        <Form.Item name="requiredRoleCode" label={t('workflow.requiredRole')}>
          <Input placeholder={t('workflow.requiredRolePlaceholder')} />
        </Form.Item>
        <Form.Item name="conditionsJson" label={t('workflow.conditions')}>
          <Input.TextArea rows={3} placeholder='{"operator":"equals","field":"status","value":"ready"}' />
        </Form.Item>
        <Form.Item
          name="defaultAssigneeId"
          label={t('workflow.defaultAssigneeId', 'Người phê duyệt mặc định')}
        >
          <Input placeholder="UUID người phê duyệt (tuỳ chọn)" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
