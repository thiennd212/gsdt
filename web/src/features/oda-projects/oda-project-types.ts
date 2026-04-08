// Re-export shared sub-entity DTOs from domestic (same BE types)
export type {
  ProjectLocationDto,
  DecisionDto,
  CapitalPlanDto,
  BidPackageDto,
  ContractDto,
  ExecutionDto,
  DisbursementDto,
  InspectionDto,
  EvaluationDto,
  AuditRecordDto,
  ViolationDto,
  OperationInfoDto,
  ProjectDocumentDto,
  SeedCatalogItem,
} from '@/features/domestic-projects/domestic-project-types';

// ── ODA-only DTOs ────────────────────────────────────────────────────────────

export interface LoanAgreementDto {
  id: string;
  agreementNumber: string;
  agreementDate: string;
  lenderName: string;
  amount: number;
  currency: string;
  interestRate: number | null;
  gracePeriod: number | null;
  repaymentPeriod: number | null;
  notes: string | null;
  fileId: string | null;
}

export interface ServiceBankDto {
  id: string;
  bankId: string;
  role: string;
  notes: string | null;
}

export interface ProcurementConditionDto {
  isBound: boolean;
  summary: string | null;
  donorApprovalRequired: boolean;
  specialConditions: string | null;
}

// ── List DTO ─────────────────────────────────────────────────────────────────

export interface OdaProjectListItem {
  id: string;
  projectCode: string;
  projectName: string;
  shortName: string;
  odaProjectTypeName: string | null;
  createdAt: string;
  statusName: string | null;
}

export interface OdaProjectListParams {
  page?: number;
  pageSize?: number;
  search?: string;
}

// ── Detail DTO ───────────────────────────────────────────────────────────────

export interface OdaProjectDetail {
  id: string;
  projectCode: string;
  projectName: string;
  managingAuthorityId: string;
  industrySectorId: string;
  projectOwnerId: string;
  projectManagementUnitId: string | null;
  pmuDirectorName: string | null;
  pmuPhone: string | null;
  pmuEmail: string | null;
  implementationPeriod: string | null;
  policyDecisionNumber: string | null;
  policyDecisionDate: string | null;
  policyDecisionAuthority: string | null;
  policyDecisionPerson: string | null;
  policyDecisionFileId: string | null;
  // ODA-specific
  shortName: string;
  projectCodeQhns: string | null;
  odaProjectTypeId: string;
  donorId: string;
  coDonorName: string | null;
  odaGrantCapital: number;
  odaLoanCapital: number;
  counterpartCentralBudget: number;
  counterpartLocalBudget: number;
  counterpartOtherCapital: number;
  totalInvestment: number;
  grantMechanismPercent: number;
  relendingMechanismPercent: number;
  statusId: string;
  procurementConditionBound: boolean;
  procurementConditionSummary: string | null;
  startYear: number | null;
  endYear: number | null;
  rowVersion: string;
  // Shared child collections
  locations: import('@/features/domestic-projects/domestic-project-types').ProjectLocationDto[];
  decisions: import('@/features/domestic-projects/domestic-project-types').DecisionDto[];
  capitalPlans: import('@/features/domestic-projects/domestic-project-types').CapitalPlanDto[];
  bidPackages: import('@/features/domestic-projects/domestic-project-types').BidPackageDto[];
  executions: import('@/features/domestic-projects/domestic-project-types').ExecutionDto[];
  disbursements: import('@/features/domestic-projects/domestic-project-types').DisbursementDto[];
  inspections: import('@/features/domestic-projects/domestic-project-types').InspectionDto[];
  evaluations: import('@/features/domestic-projects/domestic-project-types').EvaluationDto[];
  audits: import('@/features/domestic-projects/domestic-project-types').AuditRecordDto[];
  violations: import('@/features/domestic-projects/domestic-project-types').ViolationDto[];
  operation: import('@/features/domestic-projects/domestic-project-types').OperationInfoDto | null;
  documents: import('@/features/domestic-projects/domestic-project-types').ProjectDocumentDto[];
  // ODA-only
  loanAgreements: LoanAgreementDto[];
  serviceBanks: ServiceBankDto[];
  procurementCondition: ProcurementConditionDto | null;
}

// ── Create/Update requests ───────────────────────────────────────────────────

export interface CreateOdaProjectRequest {
  projectCode: string;
  projectName: string;
  shortName: string;
  managingAuthorityId: string;
  industrySectorId: string;
  projectOwnerId: string;
  odaProjectTypeId: string;
  donorId: string;
  projectManagementUnitId?: string | null;
  pmuDirectorName?: string | null;
  pmuPhone?: string | null;
  pmuEmail?: string | null;
  implementationPeriod?: string | null;
  odaGrantCapital: number;
  odaLoanCapital: number;
  counterpartCentralBudget: number;
  counterpartLocalBudget: number;
  counterpartOtherCapital: number;
  grantMechanismPercent: number;
  relendingMechanismPercent: number;
  statusId: string;
  procurementConditionBound: boolean;
  procurementConditionSummary?: string | null;
  startYear?: number | null;
  endYear?: number | null;
  projectCodeQhns?: string | null;
  coDonorName?: string | null;
  policyDecisionNumber?: string | null;
  policyDecisionDate?: string | null;
  policyDecisionAuthority?: string | null;
  policyDecisionPerson?: string | null;
  policyDecisionFileId?: string | null;
}

export interface UpdateOdaProjectRequest extends CreateOdaProjectRequest {
  id: string;
  rowVersion: string;
}
