// Forms feature types — mirrors backend DTOs

// Matches backend FormFieldType enum — grouped by phase
export type FormFieldType =
  // Basic (Phase 1)
  | 'Text'
  | 'Number'
  | 'Date'
  | 'Textarea'
  | 'Boolean'
  | 'File'
  | 'Signature'
  | 'RichText'
  // Reference (Phase 2)
  | 'EnumRef'
  | 'InternalRef'
  | 'ExternalRef'
  // Complex (Phase 2)
  | 'TableField'
  | 'AddressField'
  | 'DateRange'
  // Computed (Phase 2)
  | 'Formula'
  // UI-only (Phase 2)
  | 'Section'
  | 'Label'
  | 'Divider';

// Matches backend SubmissionStatus enum
export type SubmissionStatus = 'Pending' | 'Approved' | 'Rejected';

export interface FormFieldOptionDto {
  id: string;
  value: string;
  labelVi: string;
  labelEn: string;
  displayOrder: number;
}

export interface FormFieldValidationRulesDto {
  minLength?: number | null;
  maxLength?: number | null;
  min?: number | null;
  max?: number | null;
  pattern?: string | null;
}

export interface FormFieldDto {
  id: string;
  fieldKey: string;
  type: FormFieldType;
  labelVi: string;
  labelEn: string;
  required: boolean;
  displayOrder: number;
  isActive: boolean;
  addedInVersion: number;
  validationRules?: FormFieldValidationRulesDto | null;
  options?: FormFieldOptionDto[] | null;
  // Raw JSON strings for builder editors
  dataSourceJson?: string | null;
  formulaJson?: string | null;
  validationRulesJson?: string | null;
  /** JSON array of visibility conditions — evaluated client-side in preview */
  visibilityRulesJson?: string | null;
  /** JSON array of required-if conditions — field becomes required when conditions are met */
  requiredIfJson?: string | null;
  isPii?: boolean;
}

/** Single visibility condition: show this field when [fieldKey] [op] [value] */
export interface VisibilityCondition {
  fieldKey: string;
  operator: 'eq' | 'neq' | 'gt' | 'lt' | 'contains' | 'empty' | 'notEmpty';
  value?: string;
}

/** Top-level visibility rules wrapper */
export interface VisibilityRules {
  /** 'all' = AND, 'any' = OR */
  match: 'all' | 'any';
  conditions: VisibilityCondition[];
}

// Matches backend FormTemplateStatus enum
export type FormStatus = 'Draft' | 'Materializing' | 'Active' | 'Inactive';

export interface FormTemplateDto {
  id: string;
  code: string;
  name: string;
  nameVi?: string;
  description?: string;
  status: FormStatus;
  version: number;
  fields: FormFieldDto[];
  submissionsCount: number;
  createdAt: string;
  updatedAt?: string;
  requiresConsent?: boolean;
  consentText?: string | null;
  approvalWorkflowDefinitionId?: string | null;
}

export interface FormSubmissionDto {
  id: string;
  tenantId: string;
  formTemplateId: string;
  formTemplateVersion: number;
  submittedBy: string;
  submittedAt: string;
  data: Record<string, unknown>;
  status: SubmissionStatus;
  reviewedBy?: string | null;
  reviewedAt?: string | null;
  reviewComment?: string | null;
}

export interface FormSubmissionListItemDto {
  id: string;
  formTemplateId: string;
  formTemplateVersion: number;
  submittedBy: string;
  submittedAt: string;
  status: SubmissionStatus;
  reviewedBy?: string | null;
  reviewedAt?: string | null;
  reviewComment?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/** Payload for POST /templates/{id}/fields */
export interface AddFieldPayload {
  fieldKey: string;
  type: FormFieldType;
  labelVi: string;
  labelEn: string;
  displayOrder: number;
  required: boolean;
  validationRulesJson?: string | null;
  options?: FieldOptionPayload[] | null;
  dataSourceJson?: string | null;
  formulaJson?: string | null;
}

/** Payload for PUT /templates/{id}/fields/{fieldId} */
export interface UpdateFieldPayload {
  labelVi?: string | null;
  labelEn?: string | null;
  required?: boolean | null;
  validationRulesJson?: string | null;
  dataSourceJson?: string | null;
  formulaJson?: string | null;
  isPii?: boolean | null;
  visibilityRulesJson?: string | null;
  /** JSON array of required-if conditions — field becomes conditionally required */
  requiredIfJson?: string | null;
}

export interface FieldOptionPayload {
  value: string;
  labelVi: string;
  labelEn: string;
  displayOrder: number;
}

// --- Analytics ---
export interface FormAnalyticsDto {
  totalSubmissions: number;
  pendingCount: number;
  approvedCount: number;
  rejectedCount: number;
  avgFieldsFilled: number;
  submissionsByDate: Array<{ date: string; count: number }>;
}

// --- Version diff ---
export interface DiffFieldDto {
  fieldKey: string;
  labelVi: string;
  labelEn: string;
  addedInVersion: number;
  isActive: boolean;
}

export interface FormVersionDiffDto {
  fromVersion: number;
  toVersion: number;
  added: DiffFieldDto[];
  removed: DiffFieldDto[];
  modified: DiffFieldDto[];
}

// --- Bulk actions ---
export interface BulkActionResult {
  succeeded: number;
  failed: number;
  errors: string[];
}

// --- Submission field filter ---
export interface SubmissionFieldFilter {
  fieldKey: string;
  value: string;
}

// --- DataSource dynamic options ---
export interface DataSourceOptionDto {
  value: string;
  label: string;
  labelVi?: string;
}
