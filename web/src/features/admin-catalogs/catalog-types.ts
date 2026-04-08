// DTO for generic catalog items (Code, Name, IsActive)
export interface CatalogItemDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

// Request to create a catalog item
export interface CreateCatalogRequest {
  code: string;
  name: string;
}

// Request to update a catalog item
export interface UpdateCatalogRequest {
  code: string;
  name: string;
  isActive: boolean;
}

// DTO for KHLCNT (ContractorSelectionPlan)
export interface ContractorSelectionPlanDto {
  id: string;
  orderNumber: number;
  nameVi: string;
  nameEn: string;
  signedDate: string;
  signedBy: string;
  isActive: boolean;
}

// Request to create a KHLCNT
export interface CreateContractorSelectionPlanRequest {
  nameVi: string;
  nameEn: string;
  signedDate: string;
  signedBy: string;
}

// Request to update a KHLCNT
export interface UpdateContractorSelectionPlanRequest {
  nameVi: string;
  nameEn: string;
  signedDate: string;
  signedBy: string;
  isActive: boolean;
}

// ── GovernmentAgency ─────────────────────────────────────────────────────────
export interface GovernmentAgencyDto {
  id: string;
  name: string;
  code: string;
  parentId: string | null;
  agencyType: string | null;
  origin: string | null;
  ldaServer: string | null;
  address: string | null;
  phone: string | null;
  fax: string | null;
  email: string | null;
  notes: string | null;
  sortOrder: number;
  reportDisplayOrder: number | null;
  isActive: boolean;
}

export interface GovernmentAgencyTreeNode extends GovernmentAgencyDto {
  children: GovernmentAgencyTreeNode[];
}

export interface CreateGovernmentAgencyRequest {
  name: string;
  code: string;
  parentId?: string | null;
  agencyType?: string | null;
  origin?: string | null;
  ldaServer?: string | null;
  address?: string | null;
  phone?: string | null;
  fax?: string | null;
  email?: string | null;
  notes?: string | null;
  sortOrder?: number;
  reportDisplayOrder?: number | null;
}

export interface UpdateGovernmentAgencyRequest extends CreateGovernmentAgencyRequest {
  isActive: boolean;
}

// ── Investor ──────────────────────────────────────────────────────────────────
export interface InvestorDto {
  id: string;
  investorType: string;
  businessIdOrCccd: string;
  nameVi: string;
  nameEn: string | null;
  isActive: boolean;
}

export interface CreateInvestorRequest {
  investorType: string;
  businessIdOrCccd: string;
  nameVi: string;
  nameEn?: string | null;
}

export interface UpdateInvestorRequest extends CreateInvestorRequest {
  isActive: boolean;
}
