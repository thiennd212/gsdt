// ── List DTO ─────────────────────────────────────────────────────────────────

export interface DnnnProjectListItem {
  id: string;
  projectCode: string;
  projectName: string;
  investorName: string | null;
  competentAuthorityName: string | null;
  prelimTotalInvestment: number | null;
  statusName: string | null;
  createdAt: string;
}

export interface DnnnProjectListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  competentAuthorityId?: string;
  investorName?: string;
  statusId?: string;
  locationProvinceId?: string;
}

// ── Sub-entity DTOs ───────────────────────────────────────────────────────────

/** DNNN investment decision — capital split: Vốn CSH + ODA + TCTD = TM ĐT */
export interface DnnnInvestmentDecisionDto {
  id: string;
  decisionType: number;
  decisionNumber: string;
  decisionDate: string;
  decisionAuthority: string;
  signer: string | null;
  summary: string | null;
  totalInvestment: number;
  equityCapital: number;
  odaLoanCapital: number;
  creditLoanCapital: number;
  equityRatio: number | null;
  isAdjustment: boolean;
  isStop: boolean;
  fileId: string | null;
}

/** GCNĐKĐT — Registration Certificate for DNNN projects */
export interface RegistrationCertificateDto {
  id: string;
  certificateNumber: string;
  issuedDate: string;
  fileId: string | null;
  investmentCapital: number;
  equityCapital: number;
  equityRatio: number | null;
  notes: string | null;
}

export interface DnnnInvestorSelectionDto {
  id: string;
  selectionFormId: string | null;
  investorIds: string[];
  decisionNumber: string | null;
  decisionDate: string | null;
  fileId: string | null;
}

export interface DesignEstimateItemDto {
  id: string;
  name: string;
  scale: string | null;
  cost: number;
  fileId: string | null;
}

export interface DesignEstimateDto {
  id: string;
  decisionNumber: string;
  decisionDate: string;
  decisionAuthority: string;
  signer: string | null;
  summary: string | null;
  fileId: string | null;
  costEquipment: number;
  costConstruction: number;
  costLandCompensation: number;
  costManagement: number;
  costConsultancy: number;
  costContingency: number;
  costOther: number;
  items: DesignEstimateItemDto[];
}

export interface DnnnBidPackageDto {
  id: string;
  name: string;
  bidSelectionFormId: string;
  bidSelectionMethodId: string;
  contractFormId: string;
  bidSectorTypeId: string;
  estimatedPrice: number | null;
  winningPrice: number | null;
  winningContractorId: string | null;
  resultDecisionNumber: string | null;
  resultDecisionDate: string | null;
  notes: string | null;
}

export interface DnnnLocationDto {
  id: string;
  provinceId: string;
  districtId: string | null;
  wardId: string | null;
  address: string | null;
  industrialZoneName: string | null;
}

export interface DnnnProjectDocumentDto {
  id: string;
  documentTypeId: string;
  fileId: string;
  title: string;
  uploadedAt: string;
  notes: string | null;
}

// ── Detail DTO ────────────────────────────────────────────────────────────────

/** Minimal operation info — DNNN projects may carry basic operational data */
export interface DnnnOperationDto {
  operationDate?: string | null;
  operatingAgency?: string | null;
  revenueLastYear?: number | null;
  expenseLastYear?: number | null;
  notes?: string | null;
}

export interface DnnnProjectDetail {
  id: string;
  projectCode: string;
  projectName: string;
  investorName: string | null;
  stateOwnershipRatio: number | null;
  competentAuthorityId: string;
  industrySectorId: string;
  projectGroupId: string;
  statusId: string;
  scale: string | null;
  area: string | null;
  capacity: string | null;
  objectives: string | null;
  implementationTimeline: string | null;
  progressDescription: string | null;
  rowVersion: string;
  // Collections
  decisions: DnnnInvestmentDecisionDto[];
  certificates: RegistrationCertificateDto[];
  investorSelection: DnnnInvestorSelectionDto | null;
  designEstimates: DesignEstimateDto[];
  bidPackages: DnnnBidPackageDto[];
  locations: DnnnLocationDto[];
  documents: DnnnProjectDocumentDto[];
  operation: DnnnOperationDto | null;
  inspections: import('@/features/domestic-projects/domestic-project-types').InspectionDto[];
  evaluations: import('@/features/domestic-projects/domestic-project-types').EvaluationDto[];
  audits: import('@/features/domestic-projects/domestic-project-types').AuditRecordDto[];
  violations: import('@/features/domestic-projects/domestic-project-types').ViolationDto[];
}

// ── Request types ─────────────────────────────────────────────────────────────

export interface CreateDnnnProjectRequest {
  projectCode: string;
  projectName: string;
  competentAuthorityId: string;
  industrySectorId: string;
  projectGroupId: string;
  statusId: string;
  investorName?: string | null;
  stateOwnershipRatio?: number | null;
  scale?: string | null;
  area?: string | null;
  capacity?: string | null;
  objectives?: string | null;
  implementationTimeline?: string | null;
  progressDescription?: string | null;
}

export interface UpdateDnnnProjectRequest extends CreateDnnnProjectRequest {
  id: string;
  rowVersion: string;
}
