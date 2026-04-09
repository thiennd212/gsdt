// PermissionGate — renders children only when the current user holds the required permission.
// Use as a declarative wrapper around any UI element that requires access control.
// Falls back to `fallback` prop (default: null) when permission is absent.

import type { ReactNode } from 'react';
import { usePermissions } from '@/features/auth/use-permissions';

interface PermissionGateProps {
  /** Permission code string, e.g. "roles.manage" or "Roles.Manage" */
  permission: string;
  /** Rendered when user lacks permission — defaults to null (renders nothing) */
  fallback?: ReactNode;
  children: ReactNode;
}

export function PermissionGate({ permission, fallback = null, children }: PermissionGateProps) {
  const { hasPermission } = usePermissions();
  return hasPermission(permission) ? <>{children}</> : <>{fallback}</>;
}
