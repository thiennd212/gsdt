import { useMemo } from 'react';
import { useAuth } from './use-auth';

// Parses JWT `permission` claim (string or string[]) from user profile
export function usePermissions() {
  const { user } = useAuth();

  const permissions = useMemo<string[]>(() => {
    const raw = (user?.profile as Record<string, unknown> | undefined)
      ?.permission;
    if (!raw) return [];
    return Array.isArray(raw) ? (raw as string[]) : [raw as string];
  }, [user]);

  const roles = useMemo<string[]>(() => {
    const raw = (user?.profile as Record<string, unknown> | undefined)?.role;
    if (!raw) return [];
    return Array.isArray(raw) ? (raw as string[]) : [raw as string];
  }, [user]);

  const hasPermission = (perm: string) => permissions.includes(perm);
  const hasRole = (role: string) => roles.includes(role);

  return { permissions, roles, hasPermission, hasRole };
}
