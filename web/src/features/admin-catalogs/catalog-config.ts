import {
  BankOutlined,
  TeamOutlined,
  AuditOutlined,
  SafetyCertificateOutlined,
  FileProtectOutlined,
  ToolOutlined,
  FlagOutlined,
  ApartmentOutlined,
  SolutionOutlined,
  HomeOutlined,
  ScheduleOutlined,
  UserOutlined,
} from '@ant-design/icons';
import React from 'react';

// Metadata for each dynamic catalog — drives the generic list page
export interface CatalogMeta {
  label: string;
  description: string;
  icon: React.ReactNode;
}

// 10 generic catalogs mapped by their route/API slug
export const CATALOG_CONFIG: Record<string, CatalogMeta> = {
  'managing-authorities': {
    label: 'Cơ quan chủ quản',
    description: 'Danh mục cơ quan chủ quản dự án đầu tư',
    icon: React.createElement(ApartmentOutlined),
  },
  'national-target-programs': {
    label: 'Chương trình MTQG',
    description: 'Danh mục chương trình mục tiêu quốc gia (CTMTQG)',
    icon: React.createElement(FlagOutlined),
  },
  'project-owners': {
    label: 'Chủ đầu tư',
    description: 'Danh mục chủ đầu tư dự án',
    icon: React.createElement(TeamOutlined),
  },
  'project-management-units': {
    label: 'Ban quản lý dự án',
    description: 'Danh mục ban quản lý dự án (Ban QLDA)',
    icon: React.createElement(SolutionOutlined),
  },
  'investment-decision-authorities': {
    label: 'Cơ quan QĐ đầu tư',
    description: 'Danh mục cơ quan có thẩm quyền quyết định đầu tư',
    icon: React.createElement(SafetyCertificateOutlined),
  },
  'contractors': {
    label: 'Đơn vị nhà thầu',
    description: 'Danh mục đơn vị nhà thầu tham gia dự án',
    icon: React.createElement(ToolOutlined),
  },
  'document-types': {
    label: 'Loại văn bản',
    description: 'Danh mục loại văn bản liên quan dự án đầu tư',
    icon: React.createElement(FileProtectOutlined),
  },
  'project-implementation-statuses': {
    label: 'Trạng thái thực hiện DA',
    description: 'Danh mục trạng thái thực hiện dự án đầu tư',
    icon: React.createElement(AuditOutlined),
  },
  'banks': {
    label: 'Ngân hàng',
    description: 'Danh mục ngân hàng phục vụ giải ngân, thanh toán',
    icon: React.createElement(BankOutlined),
  },
  'managing-agencies': {
    label: 'Cơ quan quản lý',
    description: 'Danh mục cơ quan quản lý nhà nước về đầu tư',
    icon: React.createElement(HomeOutlined),
  },
};

// KHLCNT metadata (separate from generic catalogs)
export const KHLCNT_META: CatalogMeta = {
  label: 'Kế hoạch lựa chọn nhà thầu',
  description: 'Kế hoạch lựa chọn nhà thầu (KHLCNT) theo QĐ của cấp có thẩm quyền',
  icon: React.createElement(ScheduleOutlined),
};

// All catalog type slugs (for validation)
export const VALID_CATALOG_TYPES = Object.keys(CATALOG_CONFIG);

// GovernmentAgency metadata (dedicated tree page — not a generic catalog)
export const GOVERNMENT_AGENCY_META: CatalogMeta = {
  label: 'Cơ quan quản lý nhà nước',
  description: 'Danh mục cơ quan quản lý nhà nước (cấu trúc phân cấp)',
  icon: React.createElement(BankOutlined),
};

// Investor metadata (dedicated table page — not a generic catalog)
export const INVESTOR_META: CatalogMeta = {
  label: 'Nhà đầu tư',
  description: 'Danh mục nhà đầu tư (doanh nghiệp, cá nhân, tổ chức)',
  icon: React.createElement(UserOutlined),
};
