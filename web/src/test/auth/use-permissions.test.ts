import { describe, it, expect } from 'vitest';
import { renderHook } from '@testing-library/react';
import { type ReactNode, createElement } from 'react';
import { AuthContext } from '@/features/auth/auth-provider';
import type { AuthContextValue } from '@/features/auth/auth-provider';
import { usePermissions } from '@/features/auth/use-permissions';

// Build a minimal mock User with the given profile claims
function makeUser(profile: Record<string, unknown>) {
  return {
    profile,
    access_token: 'tok',
    expired: false,
  } as unknown as import('oidc-client-ts').User;
}

function makeWrapper(ctxValue: AuthContextValue) {
  return ({ children }: { children: ReactNode }) =>
    createElement(AuthContext.Provider, { value: ctxValue }, children);
}

function makeCtx(user: import('oidc-client-ts').User | null): AuthContextValue {
  return {
    user,
    isAuthenticated: !!user,
    isLoading: false,
    login: async () => {},
    logout: async () => {},
  };
}

describe('usePermissions — permission parsing', () => {
  it('returns empty permissions when user is null', () => {
    const { result } = renderHook(() => usePermissions(), {
      wrapper: makeWrapper(makeCtx(null)),
    });
    expect(result.current.permissions).toEqual([]);
    expect(result.current.hasPermission('any')).toBe(false);
  });

  it('parses a single permission string into array', () => {
    const user = makeUser({ permission: 'users.read' });
    const { result } = renderHook(() => usePermissions(), {
      wrapper: makeWrapper(makeCtx(user)),
    });
    expect(result.current.permissions).toEqual(['users.read']);
    expect(result.current.hasPermission('users.read')).toBe(true);
    expect(result.current.hasPermission('audit.read')).toBe(false);
  });

  it('parses an array of permissions correctly', () => {
    const user = makeUser({ permission: ['users.read', 'audit.read', 'cases.write'] });
    const { result } = renderHook(() => usePermissions(), {
      wrapper: makeWrapper(makeCtx(user)),
    });
    expect(result.current.permissions).toHaveLength(3);
    expect(result.current.hasPermission('audit.read')).toBe(true);
    expect(result.current.hasPermission('admin.delete')).toBe(false);
  });

  it('returns empty permissions when permission claim is absent', () => {
    const user = makeUser({ sub: 'user-123', email: 'test@gov.vn' });
    const { result } = renderHook(() => usePermissions(), {
      wrapper: makeWrapper(makeCtx(user)),
    });
    expect(result.current.permissions).toEqual([]);
  });
});

describe('usePermissions — role parsing', () => {
  it('returns empty roles when role claim is absent', () => {
    const user = makeUser({ permission: 'users.read' });
    const { result } = renderHook(() => usePermissions(), {
      wrapper: makeWrapper(makeCtx(user)),
    });
    expect(result.current.roles).toEqual([]);
    expect(result.current.hasRole('Admin')).toBe(false);
  });

  it('parses a single role string into array', () => {
    const user = makeUser({ role: 'Admin' });
    const { result } = renderHook(() => usePermissions(), {
      wrapper: makeWrapper(makeCtx(user)),
    });
    expect(result.current.roles).toEqual(['Admin']);
    expect(result.current.hasRole('Admin')).toBe(true);
  });

  it('parses an array of roles correctly', () => {
    const user = makeUser({ role: ['Admin', 'Supervisor'] });
    const { result } = renderHook(() => usePermissions(), {
      wrapper: makeWrapper(makeCtx(user)),
    });
    expect(result.current.roles).toHaveLength(2);
    expect(result.current.hasRole('Supervisor')).toBe(true);
    expect(result.current.hasRole('Unknown')).toBe(false);
  });
});
