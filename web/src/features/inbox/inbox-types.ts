import type { CaseDto, CaseStatus } from '@/features/cases/case-types';

// InboxItem — a case pending action by the current user
export interface InboxItem extends CaseDto {
  actionNeeded: InboxAction;
  assignedAt: string;
}

// InboxAction — describes what the current user needs to do
export type InboxAction = 'NeedsAssignment' | 'NeedsReview' | 'NeedsApproval' | 'NeedsRejection';

// Map from status → required action label (Vietnamese)
export const INBOX_ACTION_LABELS: Record<InboxAction, string> = {
  NeedsAssignment: 'Chờ phân công',
  NeedsReview: 'Chờ xem xét',
  NeedsApproval: 'Chờ duyệt',
  NeedsRejection: 'Chờ từ chối',
};

// Statuses that appear in the inbox (require staff action)
export const INBOX_STATUSES: CaseStatus[] = ['Submitted', 'UnderReview', 'ReturnedForRevision'];
