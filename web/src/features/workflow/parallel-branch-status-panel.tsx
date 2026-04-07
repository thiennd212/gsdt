// Parallel branch status panel — shows per-branch approval progress with inline resolve actions

import { useState } from 'react';
import { Card, Steps, Button, Space, Input, Popconfirm, Typography, message, Spin } from 'antd';
import {
  ClockCircleFilled,
  CheckCircleFilled,
  CloseCircleFilled,
  WarningFilled,
  ExclamationCircleFilled,
} from '@ant-design/icons';
import { useTranslation } from 'react-i18next';
import {
  useBranchStatuses,
  useResolveBranchChild,
  BranchResolution,
  type BranchChildStatusDto,
} from './workflow-api';

const { Text } = Typography;

interface Props {
  instanceId: string;
}

// Map resolutionType → icon + color
function getStepIcon(resolutionType: number | undefined) {
  if (resolutionType === undefined || resolutionType === null) {
    return <ClockCircleFilled style={{ color: '#aaa' }} />;
  }
  switch (resolutionType) {
    case BranchResolution.Approved:
      return <CheckCircleFilled style={{ color: '#52c41a' }} />;
    case BranchResolution.Rejected:
      return <CloseCircleFilled style={{ color: '#ff4d4f' }} />;
    case BranchResolution.Escalated:
      return <WarningFilled style={{ color: '#fa8c16' }} />;
    case BranchResolution.Timeout:
      return <ExclamationCircleFilled style={{ color: '#fa8c16' }} />;
    default:
      return <ClockCircleFilled style={{ color: '#aaa' }} />;
  }
}

// Inline approve/reject row for a pending child
interface ChildActionRowProps {
  child: BranchChildStatusDto;
  instanceId: string;
}

function ChildActionRow({ child, instanceId }: ChildActionRowProps) {
  const { t } = useTranslation();
  const [comment, setComment] = useState('');
  const resolveMutation = useResolveBranchChild(instanceId);

  const handleResolve = async (resolutionType: number) => {
    try {
      await resolveMutation.mutateAsync({
        childStatusId: child.id,
        resolutionType,
        comment: comment.trim() || undefined,
      });
      message.success(
        resolutionType === BranchResolution.Approved
          ? t('branch.approveSuccess', 'Đã phê duyệt')
          : t('branch.rejectSuccess', 'Đã từ chối'),
      );
    } catch {
      message.error(t('common.error', 'Có lỗi xảy ra'));
    }
  };

  return (
    <Space direction="vertical" size={4} style={{ width: '100%', marginTop: 4 }}>
      <Input
        size="small"
        placeholder={t('branch.commentPlaceholder', 'Ghi chú (tuỳ chọn)')}
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        style={{ maxWidth: 280 }}
      />
      <Space size="small">
        <Popconfirm
          title={t('branch.approveConfirm', 'Xác nhận phê duyệt?')}
          onConfirm={() => handleResolve(BranchResolution.Approved)}
          okText={t('common.confirm', 'Xác nhận')}
          cancelText={t('common.cancel', 'Hủy')}
        >
          <Button
            size="small"
            type="primary"
            style={{ backgroundColor: '#52c41a', borderColor: '#52c41a' }}
            loading={resolveMutation.isPending}
          >
            {t('branch.approve', 'Phê duyệt')}
          </Button>
        </Popconfirm>
        <Popconfirm
          title={t('branch.rejectConfirm', 'Xác nhận từ chối?')}
          onConfirm={() => handleResolve(BranchResolution.Rejected)}
          okText={t('common.confirm', 'Xác nhận')}
          cancelText={t('common.cancel', 'Hủy')}
          okButtonProps={{ danger: true }}
        >
          <Button size="small" danger loading={resolveMutation.isPending}>
            {t('branch.reject', 'Từ chối')}
          </Button>
        </Popconfirm>
      </Space>
    </Space>
  );
}

// Main panel — renders one Card per parallel branch
export function ParallelBranchStatusPanel({ instanceId }: Props) {
  const { t } = useTranslation();

  const { data: branches, isLoading } = useBranchStatuses(instanceId);

  // Empty state — render nothing per spec
  if (!isLoading && (!branches || branches.length === 0)) return null;

  if (isLoading) {
    return <Spin size="small" style={{ display: 'block', margin: '16px auto' }} />;
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }} size={12}>
      {branches!.map((branch) => {
        const stepItems = branch.childStatuses.map((child, idx) => {
          const isPending = child.resolutionType === undefined || child.resolutionType === null;
          return {
            key: child.id,
            title: (
              <Text style={{ fontSize: 13 }}>
                {t('branch.approver', 'Người phê duyệt')} {idx + 1}
              </Text>
            ),
            icon: getStepIcon(child.resolutionType),
            description: isPending ? (
              <ChildActionRow
                child={child}
                instanceId={instanceId}
              />
            ) : (
              child.comment && (
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {child.comment}
                </Text>
              )
            ),
          };
        });

        return (
          <Card
            key={branch.id}
            size="small"
            title={
              <Space>
                <Text strong>{t('branch.branchLabel', 'Nhánh phê duyệt')}</Text>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {branch.status}
                </Text>
              </Space>
            }
          >
            <Steps
              direction="vertical"
              size="small"
              items={stepItems}
              style={{ marginTop: 4 }}
            />
          </Card>
        );
      })}
    </Space>
  );
}
