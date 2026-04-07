import { Tag } from 'antd';
import { useTranslation } from 'react-i18next';
import { CASE_STATUS_CONFIG, CASE_PRIORITY_CONFIG } from './case-types';
import type { CaseStatus, CasePriority } from './case-types';

// CaseStatusTag — colored Ant Tag for a CaseStatus value
export function CaseStatusTag({ status }: { status: CaseStatus }) {
  const { t } = useTranslation();
  const config = CASE_STATUS_CONFIG[status] ?? { color: 'default', label: status };
  return <Tag color={config.color}>{t(config.label)}</Tag>;
}

// CasePriorityTag — colored Ant Tag for a CasePriority value
export function CasePriorityTag({ priority }: { priority: CasePriority }) {
  const { t } = useTranslation();
  const config = CASE_PRIORITY_CONFIG[priority] ?? { color: 'default', label: priority };
  return <Tag color={config.color}>{t(config.label)}</Tag>;
}
