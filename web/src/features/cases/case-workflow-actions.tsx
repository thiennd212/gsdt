import { useState } from 'react';
import { Button, Modal, Form, Input, Space, message, Popconfirm } from 'antd';
import { useTranslation } from 'react-i18next';
import { useSubmitCase, useAssignCase, useApproveCase, useRejectCase, useCloseCase } from './case-api';
import type { CaseDto } from './case-types';

interface Props {
  caseData: CaseDto;
}

// CaseWorkflowActions — contextual action buttons based on current case status
export function CaseWorkflowActions({ caseData }: Props) {
  const { t } = useTranslation();
  const { id, status } = caseData;

  const [assignOpen, setAssignOpen] = useState(false);
  const [rejectOpen, setRejectOpen] = useState(false);
  const [approveOpen, setApproveOpen] = useState(false);

  const [assignForm] = Form.useForm();
  const [rejectForm] = Form.useForm();
  const [approveForm] = Form.useForm();

  const submit = useSubmitCase();
  const assign = useAssignCase();
  const approve = useApproveCase();
  const reject = useRejectCase();
  const close = useCloseCase();

  async function handleSubmit() {
    try {
      await submit.mutateAsync(id);
      message.success(t('page.cases.workflow.submitSuccess'));
    } catch { message.error(t('page.cases.workflow.submitError')); }
  }

  async function handleAssign() {
    const values = await assignForm.validateFields();
    try {
      await assign.mutateAsync({ id, body: { assigneeId: values.assigneeId, department: values.department } });
      message.success(t('page.cases.workflow.assignSuccess'));
      setAssignOpen(false);
      assignForm.resetFields();
    } catch { message.error(t('page.cases.workflow.assignError')); }
  }

  async function handleApprove() {
    const values = await approveForm.validateFields();
    try {
      await approve.mutateAsync({ id, body: { reason: values.reason } });
      message.success(t('page.cases.workflow.approveSuccess'));
      setApproveOpen(false);
      approveForm.resetFields();
    } catch { message.error(t('page.cases.workflow.approveError')); }
  }

  async function handleReject() {
    const values = await rejectForm.validateFields();
    try {
      await reject.mutateAsync({ id, body: { reason: values.reason } });
      message.success(t('page.cases.workflow.rejectSuccess'));
      setRejectOpen(false);
      rejectForm.resetFields();
    } catch { message.error(t('page.cases.workflow.rejectError')); }
  }

  async function handleClose() {
    try {
      await close.mutateAsync(id);
      message.success(t('page.cases.workflow.closeSuccess'));
    } catch { message.error(t('page.cases.workflow.closeError')); }
  }

  return (
    <>
      <Space wrap>
        {status === 'Draft' && (
          <Popconfirm title={t('page.cases.workflow.submitConfirm')} okText={t('page.cases.workflow.submitBtn')} cancelText={t('common.cancel')} onConfirm={handleSubmit}>
            <Button type="primary" loading={submit.isPending}>{t('page.cases.workflow.submitBtn')}</Button>
          </Popconfirm>
        )}
        {status === 'Submitted' && (
          <Button type="primary" onClick={() => setAssignOpen(true)}>{t('page.cases.workflow.assignBtn')}</Button>
        )}
        {(status === 'UnderReview' || status === 'ReturnedForRevision') && (
          <>
            <Button type="primary" onClick={() => setApproveOpen(true)}>{t('page.cases.workflow.approveBtn')}</Button>
            <Button danger onClick={() => setRejectOpen(true)}>{t('page.cases.workflow.rejectBtn')}</Button>
          </>
        )}
        {(status === 'Approved' || status === 'Rejected') && (
          <Popconfirm title={t('page.cases.workflow.closeConfirm')} okText={t('page.cases.workflow.closeBtn')} cancelText={t('common.cancel')} onConfirm={handleClose}>
            <Button loading={close.isPending}>{t('page.cases.workflow.closeBtn')}</Button>
          </Popconfirm>
        )}
      </Space>

      {/* Assign modal */}
      <Modal title={t('page.cases.workflow.assignModalTitle')} open={assignOpen} onOk={handleAssign}
        onCancel={() => setAssignOpen(false)} okText={t('page.cases.workflow.assignBtn')} cancelText={t('common.cancel')}
        confirmLoading={assign.isPending} destroyOnHidden>
        <Form form={assignForm} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item name="assigneeId" label={t('page.cases.workflow.assigneeIdLabel')} rules={[{ required: true, message: t('page.cases.workflow.assigneeIdRequired') }]}>
            <Input placeholder={t('page.cases.workflow.assigneeIdPlaceholder')} />
          </Form.Item>
          <Form.Item name="department" label={t('page.cases.workflow.departmentLabel')} rules={[{ required: true, message: t('page.cases.workflow.departmentRequired') }]}>
            <Input placeholder={t('page.cases.workflow.departmentPlaceholder')} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Approve modal */}
      <Modal title={t('page.cases.workflow.approveModalTitle')} open={approveOpen} onOk={handleApprove}
        onCancel={() => setApproveOpen(false)} okText={t('page.cases.workflow.approveConfirmBtn')} cancelText={t('common.cancel')}
        confirmLoading={approve.isPending} destroyOnHidden>
        <Form form={approveForm} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item name="reason" label={t('page.cases.workflow.reasonLabel')} rules={[{ required: true, message: t('page.cases.workflow.reasonRequired') }]}>
            <Input.TextArea rows={3} placeholder={t('page.cases.workflow.approveReasonPlaceholder')} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Reject modal */}
      <Modal title={t('page.cases.workflow.rejectModalTitle')} open={rejectOpen} onOk={handleReject}
        onCancel={() => setRejectOpen(false)} okText={t('page.cases.workflow.rejectConfirmBtn')} okButtonProps={{ danger: true }}
        cancelText={t('common.cancel')} confirmLoading={reject.isPending} destroyOnHidden>
        <Form form={rejectForm} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item name="reason" label={t('page.cases.workflow.rejectReasonLabel')} rules={[{ required: true, message: t('page.cases.workflow.rejectReasonRequired') }]}>
            <Input.TextArea rows={3} placeholder={t('page.cases.workflow.rejectReasonPlaceholder')} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
