// ── Enums ────────────────────────────────────────────────────────────────────

export enum PppContractType {
  BOT = 1,
  BT_Land = 2,
  BT_Money = 3,
  BT_NoPayment = 4,
  BTO = 5,
  BOO = 6,
  OM = 7,
  BTL = 8,
  BLT = 9,
  Mixed = 10,
}

export const PPP_CONTRACT_TYPE_LABELS: Record<PppContractType, string> = {
  [PppContractType.BOT]: 'BOT',
  [PppContractType.BT_Land]: 'BT (đất)',
  [PppContractType.BT_Money]: 'BT (tiền)',
  [PppContractType.BT_NoPayment]: 'BT (không thanh toán)',
  [PppContractType.BTO]: 'BTO',
  [PppContractType.BOO]: 'BOO',
  [PppContractType.OM]: 'O&M',
  [PppContractType.BTL]: 'BTL',
  [PppContractType.BLT]: 'BLT',
  [PppContractType.Mixed]: 'Hỗn hợp',
};

// ── List DTO ─────────────────────────────────────────────────────────────────

export interface PppProjectListItem {
  id: string;
  projectCode: string;
  projectName: string;
  contractType: PppContractType;
  competentAuthorityName: string | null;
  latestDecisionNumber: string | null;
  latestDecisionDate: string | null;
  statusName: string | null;
}

export interface PppProjectListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  managingAuthorityId?: string;
  projectOwnerId?: string;
  industrySectorId?: string;
  projectGroupId?: string;
  statusId?: string;
  contractType?: number;
  isSubProject?: boolean;
}

// ── Sub-entity DTOs ───────────────────────────────────────────────────────────

export interface PppInvestmentDecisionDto {
  id: string;
  decisionType: number;
  decisionNumber: string;
  decisionDate: string;
  decisionAuthority: string;
  signer: string | null;
  summary: string | null;
  totalInvestment: number;
  vonNN: number;
  nsTW: number;
  nsDiaPhuong: number;
  nsNhaNuocKhac: number;
  vonCSH: number;
  vonVay: number;
  isAdjustment: boolean;
  isStop: boolean;
  fileId: string | null;
}

export interface InvestorSelectionDto {
  id: string;
  selectionFormId: string | null;
  investorIds: string[];
  decisionNumber: string | null;
  decisionDate: string | null;
  fileId: string | null;
}

export interface PppContractInfoDto {
  id: string;
  contractNumber: string | null;
  contractDate: string | null;
  duration: number | null;
  durationUnit: number | null;
  startDate: string | null;
  endDate: string | null;
  totalInvestment: number;
  vonNN: number;
  nsTW: number;
  nsDiaPhuong: number;
  nsNhaNuocKhac: number;
  vonCSH: number;
  vonVay: number;
  notes: string | null;
}

export interface PppCapitalPlanDto {
  id: string;
  decisionType: number;
  decisionNumber: string;
  decisionDate: string;
  totalAmount: number;
  notes: string | null;
  fileId: string | null;
}

export interface PppExecutionRecordDto {
  id: string;
  reportDate: string;
  progressStatus: number;
  physicalProgressPercent: number | null;
  notes: string | null;
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

export interface PppDisbursementRecordDto {
  id: string;
  reportDate: string;
  vonNNPeriod: number;
  vonNNCumulative: number;
  vonCSHPeriod: number;
  vonCSHCumulative: number;
  vonVayPeriod: number;
  vonVayCumulative: number;
}

export interface RevenueReportDto {
  id: string;
  year: number;
  period: number;
  revenuePeriod: number;
  revenueCumulative: number;
  sharingChange: string | null;
  difficulties: string | null;
  recommendations: string | null;
}

export interface PppOperationDto {
  operationDate: string | null;
  operatingAgency: string | null;
  notes: string | null;
}

export interface PppProjectDocumentDto {
  id: string;
  documentTypeId: string;
  fileId: string;
  title: string;
  uploadedAt: string;
  notes: string | null;
}

export interface PppLocationDto {
  id: string;
  provinceId: string;
  districtId: string | null;
  wardId: string | null;
  address: string | null;
}

export interface PppBidPackageDto {
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

// ── Detail DTO ────────────────────────────────────────────────────────────────

export interface PppProjectDetail {
  id: string;
  projectCode: string;
  projectName: string;
  contractType: PppContractType;
  managingAuthorityId: string;
  industrySectorId: string;
  projectOwnerId: string;
  projectGroupId: string;
  statusId: string;
  subProjectType: number;
  scale: string | null;
  area: string | null;
  capacity: string | null;
  objectives: string | null;
  implementationPeriod: string | null;
  rowVersion: string;
  // Collections
  decisions: PppInvestmentDecisionDto[];
  investorSelection: InvestorSelectionDto | null;
  contractInfo: PppContractInfoDto | null;
  capitalPlans: PppCapitalPlanDto[];
  bidPackages: PppBidPackageDto[];
  designEstimates: DesignEstimateDto[];
  executionRecords: PppExecutionRecordDto[];
  disbursements: PppDisbursementRecordDto[];
  revenueReports: RevenueReportDto[];
  operation: PppOperationDto | null;
  locations: PppLocationDto[];
  documents: PppProjectDocumentDto[];
}

// ── Request types ─────────────────────────────────────────────────────────────

export interface CreatePppProjectRequest {
  projectCode: string;
  projectName: string;
  contractType: PppContractType;
  managingAuthorityId: string;
  industrySectorId: string;
  projectOwnerId: string;
  projectGroupId: string;
  statusId: string;
  subProjectType?: number;
  scale?: string | null;
  area?: string | null;
  capacity?: string | null;
  objectives?: string | null;
  implementationPeriod?: string | null;
}

export interface UpdatePppProjectRequest extends CreatePppProjectRequest {
  id: string;
  rowVersion: string;
}
