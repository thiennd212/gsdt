// Standard API response envelope matching the backend ApiResponse<T> contract
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string | null;
  errors: ApiError[] | null;
}

// Paginated list wrapper — matches backend PaginatedResult<T>
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiError {
  code: string;
  message: string;
  field?: string;
}

// Common query params for list endpoints
export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  search?: string;
}
