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
