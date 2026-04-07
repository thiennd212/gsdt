import type { PaginationParams } from '@/shared/types/api';

// UserRole — matches backend IdentityRole values
export enum UserRole {
  SystemAdmin = 'SystemAdmin',
  Admin = 'Admin',
  GovOfficer = 'GovOfficer',
  Citizen = 'Citizen',
  Viewer = 'Viewer',
}

// UserStatus — account lifecycle state
export type UserStatus = 'Active' | 'Locked' | 'Pending' | 'Deactivated';

// UserDto — matches BE Identity.Application.DTOs.UserDto
export interface UserDto {
  id: string;
  fullName: string;
  email: string;
  departmentCode?: string;
  clearanceLevel: string;
  isActive: boolean;
  tenantId?: string;
  createdAtUtc: string;
  lastLoginAt?: string;
  passwordExpiresAt?: string;
  roles: string[];
}

// CreateUserRequest — POST /api/v1/admin/users
export interface CreateUserRequest {
  fullName: string;
  email: string;
  phoneNumber?: string;
  department?: string;
  orgUnitId?: string;
  roles: UserRole[];
  temporaryPassword?: string;
}

// UpdateUserRequest — PUT /api/v1/admin/users/{id}
export interface UpdateUserRequest {
  fullName?: string;
  phoneNumber?: string;
  department?: string;
  orgUnitId?: string;
  roles?: UserRole[];
  status?: UserStatus;
}

// UserListParams — query params for GET /api/v1/admin/users
export interface UserListParams extends PaginationParams {
  role?: UserRole;
  status?: UserStatus;
}
