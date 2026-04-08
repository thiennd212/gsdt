// ── List DTO ─────────────────────────────────────────────────────────────────

export interface DomesticProjectListItem {
  id: string;
  projectCode: string;
  projectName: string;
  latestDecisionNumber: string | null;
  latestDecisionDate: string | null;
  projectOwnerName: string | null;
  statusName: string | null;
}

export interface DomesticProjectListParams {
  page?: number;
  pageSize?: number;
  search?: string;
}

// ── Detail DTO ───────────────────────────────────────────────────────────────

export interface DomesticProjectDetail {
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
  subProjectType: number;
  treasuryCode: string | null;
  projectGroupId: string;
  prelimCentralBudget: number;
  prelimLocalBudget: number;
  prelimOtherPublicCapital: number;
  prelimPublicInvestment: number;
  prelimOtherCapital: number;
  prelimTotalInvestment: number;
  statusId: string;
  nationalTargetProgramId: string | null;
  stopContent: string | null;
  stopDecisionNumber: string | null;
  stopDecisionDate: string | null;
  stopFileId: string | null;
  rowVersion: string;
  // Child collections
  locations: ProjectLocationDto[];
  decisions: DecisionDto[];
  capitalPlans: CapitalPlanDto[];
  bidPackages: BidPackageDto[];
  executions: ExecutionDto[];
  disbursements: DisbursementDto[];
  inspections: InspectionDto[];
  evaluations: EvaluationDto[];
  audits: AuditRecordDto[];
  violations: ViolationDto[];
  operation: OperationInfoDto | null;
  documents: ProjectDocumentDto[];
}

// ── Create/Update requests ───────────────────────────────────────────────────

export interface CreateDomesticProjectRequest {
  projectCode: string;
  projectName: string;
  managingAuthorityId: string;
  industrySectorId: string;
  projectOwnerId: string;
  projectGroupId: string;
  subProjectType: number;
  projectManagementUnitId?: string | null;
  pmuDirectorName?: string | null;
  pmuPhone?: string | null;
  pmuEmail?: string | null;
  implementationPeriod?: string | null;
  prelimCentralBudget: number;
  prelimLocalBudget: number;
  prelimOtherPublicCapital: number;
  prelimOtherCapital: number;
  statusId: string;
  nationalTargetProgramId?: string | null;
  treasuryCode?: string | null;
  policyDecisionNumber?: string | null;
  policyDecisionDate?: string | null;
  policyDecisionAuthority?: string | null;
  policyDecisionPerson?: string | null;
  policyDecisionFileId?: string | null;
}

export interface UpdateDomesticProjectRequest extends CreateDomesticProjectRequest {
  id: string;
  rowVersion: string;
  stopContent?: string | null;
  stopDecisionNumber?: string | null;
  stopDecisionDate?: string | null;
  stopFileId?: string | null;
}

// ── Shared sub-entity DTOs ───────────────────────────────────────────────────

export interface ProjectLocationDto {
  id: string;
  provinceId: string;
  districtId: string | null;
  wardId: string | null;
  address: string | null;
}

export interface DecisionDto {
  id: string;
  decisionType: number;
  decisionNumber: string;
  decisionDate: string;
  decisionAuthority: string;
  totalInvestment: number;
  centralBudget: number;
  localBudget: number;
  otherPublicCapital: number;
  otherCapital: number;
  adjustmentContentId: string | null;
  notes: string | null;
  fileId: string | null;
}

export interface CapitalPlanDto {
  id: string;
  decisionType: number;
  allocationRound: number;
  decisionNumber: string;
  decisionDate: string;
  totalAmount: number;
  centralBudget: number;
  localBudget: number;
  notes: string | null;
  fileId: string | null;
}

export interface BidPackageDto {
  id: string;
  name: string;
  contractorSelectionPlanId: string | null;
  isDesignReview: boolean;
  isSupervision: boolean;
  bidSelectionFormId: string;
  bidSelectionMethodId: string;
  contractFormId: string;
  bidSectorTypeId: string;
  duration: number | null;
  durationUnit: number | null;
  estimatedPrice: number | null;
  winningPrice: number | null;
  winningContractorId: string | null;
  resultDecisionNumber: string | null;
  resultDecisionDate: string | null;
  resultFileId: string | null;
  notes: string | null;
  items: BidItemDto[];
  contracts: ContractDto[];
}

export interface BidItemDto {
  id: string;
  name: string;
  quantity: number | null;
  unit: string | null;
  estimatedPrice: number | null;
  notes: string | null;
}

export interface ContractDto {
  id: string;
  contractNumber: string;
  contractDate: string;
  contractorId: string;
  contractValue: number;
  contractFormId: string;
  notes: string | null;
  fileId: string | null;
}

export interface ExecutionDto {
  id: string;
  reportDate: string;
  bidPackageId: string | null;
  contractId: string | null;
  progressStatus: number;
  physicalProgressPercent: number | null;
  notes: string | null;
}

export interface DisbursementDto {
  id: string;
  reportDate: string;
  bidPackageId: string | null;
  contractId: string | null;
  publicCapitalMonthly: number;
  publicCapitalPreviousMonth: number | null;
  publicCapitalYtd: number;
  otherCapital: number | null;
}

export interface InspectionDto {
  id: string;
  inspectionDate: string;
  inspectionAgency: string;
  content: string;
  conclusion: string | null;
  fileId: string | null;
}

export interface EvaluationDto {
  id: string;
  evaluationDate: string;
  evaluationTypeId: string;
  content: string;
  result: string | null;
  fileId: string | null;
}

export interface AuditRecordDto {
  id: string;
  auditDate: string;
  auditAgency: string;
  conclusionTypeId: string;
  content: string;
  fileId: string | null;
}

export interface ViolationDto {
  id: string;
  violationDate: string;
  violationTypeId: string;
  content: string;
  violationActionId: string;
  penalty: number | null;
  notes: string | null;
  fileId: string | null;
}

export interface OperationInfoDto {
  operationDate: string | null;
  operatingAgency: string | null;
  revenueLastYear: number | null;
  expenseLastYear: number | null;
  notes: string | null;
}

export interface ProjectDocumentDto {
  id: string;
  documentTypeId: string;
  fileId: string;
  title: string;
  uploadedAt: string;
  notes: string | null;
}

// ── Seed catalog item (generic for all dropdowns) ────────────────────────────

export interface SeedCatalogItem {
  id: string;
  code: string;
  name: string;
  sortOrder: number;
}
