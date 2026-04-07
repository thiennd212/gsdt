import { Descriptions, Card, Spin, Alert, Typography, Space, Divider, Row, Col, Steps } from 'antd';
import { ArrowLeftOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useCase } from './case-api';
import { CaseStatusTag, CasePriorityTag } from './case-status-tag';
import { CaseWorkflowActions } from './case-workflow-actions';
import { CaseComments } from './case-comments';
import { CASE_TYPE_LABELS } from './case-types';
import type { CaseComment, CaseStatus } from './case-types';

const { Title, Text } = Typography;

// Lifecycle step statuses — order matches workflow progression
const WORKFLOW_STEP_STATUSES = [
  ['Draft'],
  ['Submitted'],
  ['UnderReview', 'ReturnedForRevision'],
  ['Approved', 'Rejected'],
  ['Closed'],
] as const;

interface WorkflowProgressProps {
  status: CaseStatus;
  stepTitles: string[];
}

// CaseWorkflowProgress — horizontal Steps bar mapping case status to lifecycle position
function CaseWorkflowProgress({ status, stepTitles }: WorkflowProgressProps) {
  const currentIndex = WORKFLOW_STEP_STATUSES.findIndex((statuses) =>
    (statuses as readonly string[]).includes(status)
  );
  const isRejected = status === 'Rejected';

  return (
    <Steps
      size="small"
      current={currentIndex}
      status={isRejected ? 'error' : 'process'}
      style={{ marginBottom: 16 }}
      items={stepTitles.map((title) => ({ title }))}
    />
  );
}

interface CaseDetailPageProps {
  caseId: string;
}

// CaseDetailPage — full case view: header, info, workflow actions, comments
export function CaseDetailPage({ caseId }: CaseDetailPageProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data: caseData, isLoading, isError } = useCase(caseId);

  if (isLoading) return <Spin tip={t('common.loading')} style={{ display: 'block', margin: '80px auto' }} />;
  if (isError || !caseData) {
    return <Alert type="error" message={t('page.caseDetail.errorMessage')} style={{ margin: 24 }} />;
  }

  // Backend may include comments in the response projection
  const comments: CaseComment[] = 'comments' in caseData
    ? ((caseData as Record<string, unknown>).comments as CaseComment[] | undefined) ?? []
    : [];

  // Translated workflow step titles — order must match WORKFLOW_STEP_STATUSES
  const workflowStepTitles = [
    t('page.caseDetail.workflow.draft'),
    t('page.caseDetail.workflow.submitted'),
    t('page.caseDetail.workflow.underReview'),
    t('page.caseDetail.workflow.verdict'),
    t('page.caseDetail.workflow.closed'),
  ];

  return (
    <Row>
      {/* Constrain detail content to lg:16/24 columns for readability on wide screens */}
      <Col xs={24} lg={16}>
      {/* Back navigation */}
      <button
        type="button"
        style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 16, cursor: 'pointer', border: 'none', background: 'none', padding: 0 }}
        onClick={() => navigate({ to: '/cases', search: { page: 1, pageSize: 20 } })}
      >
        <ArrowLeftOutlined />
        <Text type="secondary">{t('page.caseDetail.backToList')}</Text>
      </button>

      {/* Workflow progress */}
      <CaseWorkflowProgress status={caseData.status} stepTitles={workflowStepTitles} />

      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 12 }}>
          <div>
            <Space size={8} style={{ marginBottom: 4 }}>
              <Text type="secondary" style={{ fontSize: 13 }}>{caseData.caseNumber}</Text>
              <Text type="secondary" style={{ fontSize: 12 }}>#{caseData.trackingCode}</Text>
            </Space>
            <Title level={4} style={{ margin: 0 }}>{caseData.title}</Title>
            <Space size={8} style={{ marginTop: 8 }}>
              <CaseStatusTag status={caseData.status} />
              <CasePriorityTag priority={caseData.priority} />
            </Space>
          </div>
          <CaseWorkflowActions caseData={caseData} />
        </div>
      </Card>

      {/* Info */}
      <Card title={t('page.caseDetail.sectionInfo')} style={{ marginBottom: 16 }}>
        <Descriptions column={{ xs: 1, sm: 2 }} size="small">
          <Descriptions.Item label={t('page.caseDetail.label.type')}>
            {t(CASE_TYPE_LABELS[caseData.type])}
          </Descriptions.Item>
          <Descriptions.Item label={t('page.caseDetail.label.createdAt')}>
            {dayjs(caseData.createdAt).format('DD/MM/YYYY HH:mm')}
          </Descriptions.Item>
          <Descriptions.Item label={t('page.caseDetail.label.department')}>
            {caseData.assignedToDepartment ?? '—'}
          </Descriptions.Item>
          <Descriptions.Item label={t('page.caseDetail.label.assignedAt')}>
            {caseData.assignedAtUtc ? dayjs(caseData.assignedAtUtc).format('DD/MM/YYYY HH:mm') : '—'}
          </Descriptions.Item>
          {caseData.resolutionReason && (
            <Descriptions.Item label={t('page.caseDetail.label.resolutionReason')} span={2}>
              {caseData.resolutionReason}
            </Descriptions.Item>
          )}
          <Descriptions.Item label={t('page.caseDetail.label.description')} span={2}>
            <Text style={{ whiteSpace: 'pre-wrap' }}>{caseData.description}</Text>
          </Descriptions.Item>
        </Descriptions>
      </Card>

      {/* Comments */}
      <Card title={t('page.caseDetail.sectionComments')}>
        <Divider style={{ margin: '0 0 12px' }} />
        <CaseComments caseId={caseId} comments={comments} />
      </Card>
      </Col>
    </Row>
  );
}
