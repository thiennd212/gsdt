// Notifications feature types — mirrors backend DTOs

export interface NotificationDto {
  id: string;
  title: string;
  body: string;
  isRead: boolean;
  createdAt: string;
  deepLink?: string;
  channel?: string;
}

export interface UnreadCountDto {
  count: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Notification template for admin management (Liquid templates)
export interface NotificationTemplateDto {
  id: string;
  name: string;
  channel: 'Email' | 'SMS' | 'InApp' | 'Push';
  subject?: string;
  body: string;
  isActive: boolean;
  createdAt: string;
}
