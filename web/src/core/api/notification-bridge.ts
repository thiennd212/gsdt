import type { NotificationInstance } from 'antd/es/notification/interface';

// Bridge: allows non-React code (query-config) to use App-scoped notification
// Set from React tree via useNotificationBridge() hook in providers.tsx
let instance: NotificationInstance | null = null;

export function setNotificationInstance(n: NotificationInstance) {
  instance = n;
}

export function getNotificationInstance(): NotificationInstance | null {
  return instance;
}
