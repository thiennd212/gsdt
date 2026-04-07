import { Modal, Form, Input, Select, message } from 'antd';
import { useNavigate } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import { useCreateCase } from './case-api';
import { CASE_TYPE_LABELS, CASE_PRIORITY_CONFIG } from './case-types';
import type { CreateCaseRequest, CaseType, CasePriority } from './case-types';

const { TextArea } = Input;
const { Option } = Select;

interface CaseCreateFormProps {
  open: boolean;
  onClose: () => void;
}

// CaseCreateForm — modal form for creating a new case (Draft status)
export function CaseCreateForm({ open, onClose }: CaseCreateFormProps) {
  const { t } = useTranslation();
  const [form] = Form.useForm<CreateCaseRequest>();
  const createCase = useCreateCase();
  const navigate = useNavigate();

  async function handleOk() {
    try {
      const values = await form.validateFields();
      const result = await createCase.mutateAsync(values);
      message.success(t('page.cases.create.successMessage', { caseNumber: result.caseNumber }));
      form.resetFields();
      onClose();
      navigate({ to: '/cases/$caseId', params: { caseId: result.id } });
    } catch {
      // validation errors shown inline by Ant Form
    }
  }

  function handleCancel() {
    form.resetFields();
    onClose();
  }

  return (
    <Modal
      title={t('page.cases.create.modalTitle')}
      open={open}
      onOk={handleOk}
      onCancel={handleCancel}
      okText={t('page.cases.create.okBtn')}
      cancelText={t('common.cancel')}
      confirmLoading={createCase.isPending}
      destroyOnHidden
      width={560}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="title"
          label={t('page.cases.create.titleLabel')}
          rules={[{ required: true, message: t('page.cases.create.titleRequired') }, { max: 200 }]}
        >
          <Input placeholder={t('page.cases.create.titlePlaceholder')} />
        </Form.Item>

        <Form.Item
          name="description"
          label={t('page.cases.create.descriptionLabel')}
          rules={[{ required: true, message: t('page.cases.create.descriptionRequired') }]}
        >
          <TextArea rows={4} placeholder={t('page.cases.create.descriptionPlaceholder')} />
        </Form.Item>

        <Form.Item
          name="type"
          label={t('page.cases.create.typeLabel')}
          rules={[{ required: true, message: t('page.cases.create.typeRequired') }]}
        >
          <Select placeholder={t('page.cases.create.typeSelectPlaceholder')}>
            {(Object.keys(CASE_TYPE_LABELS) as CaseType[]).map((ct) => (
              <Option key={ct} value={ct}>{t(CASE_TYPE_LABELS[ct])}</Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="priority"
          label={t('page.cases.create.priorityLabel')}
          rules={[{ required: true, message: t('page.cases.create.priorityRequired') }]}
        >
          <Select placeholder={t('page.cases.create.prioritySelectPlaceholder')}>
            {(Object.keys(CASE_PRIORITY_CONFIG) as CasePriority[]).map((p) => (
              <Option key={p} value={p}>{t(CASE_PRIORITY_CONFIG[p].label)}</Option>
            ))}
          </Select>
        </Form.Item>
      </Form>
    </Modal>
  );
}
