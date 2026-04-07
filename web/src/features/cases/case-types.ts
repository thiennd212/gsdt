import type { PaginationParams } from '@/shared/types/api';

// CaseStatus — matches backend CaseStatus enum
export type CaseStatus = 'Draft' | 'Submitted' | 'UnderReview' | 'ReturnedForRevision' | 'Approved' | 'Rejected' | 'Closed';

// CasePriority — matches backend CasePriority enum
export type CasePriority = 'Low' | 'Medium' | 'High' | 'Critical';

// CaseType — matches backend CaseType enum
export type CaseType = 'Application' | 'Complaint' | 'Request' | 'Report';

// CaseDto — backend Case response shape
export interface CaseDto {
  id: string;
  caseNumber: string;
  trackingCode: string;
  title: string;
  description: string;
  status: CaseStatus;
  type: CaseType;
  priority: CasePriority;
  tenantId: string;
  assignedToId?: string;
  assignedToDepartment?: string;
  assignedAtUtc?: string;
  resolutionReason?: string;
  createdAt: string;
  updatedAt?: string;
}

// CaseComment — comment on a case
export interface CaseComment {
  id: string;
  caseId: string;
  content: string;
  authorId: string;
  authorName?: string;
  createdAt: string;
}

// CaseListParams — query params for GET /api/v1/cases
export interface CaseListParams extends PaginationParams {
  status?: CaseStatus;
  type?: CaseType;
  priority?: CasePriority;
  assignedToId?: string;
  fromDate?: string;
  toDate?: string;
}

// CreateCaseRequest — POST /api/v1/cases body
export interface CreateCaseRequest {
  title: string;
  description: string;
  type: CaseType;
  priority: CasePriority;
}

// AssignCaseRequest — POST /api/v1/cases/{id}/assign body
export interface AssignCaseRequest {
  assigneeId: string;
  department: string;
}

// ResolveRequest — POST /api/v1/cases/{id}/approve or /reject body
export interface ResolveRequest {
  reason: string;
}

// AddCommentRequest — POST /api/v1/cases/{id}/comments body
export interface AddCommentRequest {
  content: string;
  mentionedUserIds?: string[];
}

// Status display config — colors + i18n label keys (resolve via t('page.cases.status.<Status>'))
export const CASE_STATUS_CONFIG: Record<CaseStatus, { color: string; label: string }> = {
  Draft: { color: 'default', label: 'page.cases.status.Draft' },
  Submitted: { color: 'blue', label: 'page.cases.status.Submitted' },
  UnderReview: { color: 'orange', label: 'page.cases.status.UnderReview' },
  ReturnedForRevision: { color: 'cyan', label: 'page.cases.status.ReturnedForRevision' },
  Approved: { color: 'green', label: 'page.cases.status.Approved' },
  Rejected: { color: 'red', label: 'page.cases.status.Rejected' },
  Closed: { color: 'default', label: 'page.cases.status.Closed' },
};

// Priority display config — colors + i18n label keys
export const CASE_PRIORITY_CONFIG: Record<CasePriority, { color: string; label: string }> = {
  Low: { color: 'default', label: 'page.cases.priority.Low' },
  Medium: { color: 'blue', label: 'page.cases.priority.Medium' },
  High: { color: 'orange', label: 'page.cases.priority.High' },
  Critical: { color: 'red', label: 'page.cases.priority.Critical' },
};

// Type i18n label keys
export const CASE_TYPE_LABELS: Record<CaseType, string> = {
  Application: 'page.cases.type.Application',
  Complaint: 'page.cases.type.Complaint',
  Request: 'page.cases.type.Request',
  Report: 'page.cases.type.Report',
};
