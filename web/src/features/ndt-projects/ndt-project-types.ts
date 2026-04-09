// ── List DTO ─────────────────────────────────────────────────────────────────

export interface NdtProjectListItem {
  id: string;
  projectCode: string;
  projectName: string;
  investorName: string | null;
  competentAuthorityName: string | null;
  prelimTotalInvestment: number | null;
  statusName: string | null;
  createdAt: string;
}

export interface NdtProjectListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  competentAuthorityId?: string;
  investorName?: string;
  statusId?: string;
  locationProvinceId?: string;
}

// ── Sub-entity DTOs ───────────────────────────────────────────────────────────

/** NĐT investment decision — capital split: Vốn CSH + ODA + TCTD = TM ĐT */
export interface NdtInvestmentDecisionDto {
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

/** GCNĐKĐT — Registration Certificate for NĐT projects */
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

export interface NdtBidPackageDto {
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

export interface NdtLocationDto {
  id: string;
  provinceId: string;
  districtId: string | null;
  wardId: string | null;
  address: string | null;
  industrialZoneName: string | null;
}

export interface NdtProjectDocumentDto {
  id: string;
  documentTypeId: string;
  fileId: string;
  title: string;
  uploadedAt: string;
  notes: string | null;
}

// ── Detail DTO ────────────────────────────────────────────────────────────────

/** Minimal operation info — NĐT projects may carry basic operational data */
export interface NdtOperationDto {
  operationDate?: string | null;
  operatingAgency?: string | null;
  revenueLastYear?: number | null;
  expenseLastYear?: number | null;
  notes?: string | null;
}

export interface NdtProjectDetail {
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
  decisions: NdtInvestmentDecisionDto[];
  certificates: RegistrationCertificateDto[];
  bidPackages: NdtBidPackageDto[];
  locations: NdtLocationDto[];
  documents: NdtProjectDocumentDto[];
  operation: NdtOperationDto | null;
  inspections: import('@/features/domestic-projects/domestic-project-types').InspectionDto[];
  evaluations: import('@/features/domestic-projects/domestic-project-types').EvaluationDto[];
  audits: import('@/features/domestic-projects/domestic-project-types').AuditRecordDto[];
  violations: import('@/features/domestic-projects/domestic-project-types').ViolationDto[];
}

// ── Request types ─────────────────────────────────────────────────────────────

export interface CreateNdtProjectRequest {
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

export interface UpdateNdtProjectRequest extends CreateNdtProjectRequest {
  id: string;
  rowVersion: string;
}
