// Unit tests for PermissionGate component
// Verifies conditional rendering based on user permission claims

import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PermissionGate } from '@/shared/components/permission-gate';

// ─── Mock usePermissions ──────────────────────────────────────────────────────

const mockHasPermission = vi.fn<(perm: string) => boolean>();

vi.mock('@/features/auth/use-permissions', () => ({
  usePermissions: () => ({
    hasPermission: mockHasPermission,
    permissions: [],
    roles: [],
    hasRole: vi.fn(),
  }),
}));

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('PermissionGate', () => {
  beforeEach(() => {
    mockHasPermission.mockReset();
  });

  it('renders children when user has the required permission', () => {
    mockHasPermission.mockReturnValue(true);

    render(
      <PermissionGate permission="roles.manage">
        <span>Protected content</span>
      </PermissionGate>,
    );

    expect(screen.getByText('Protected content')).toBeInTheDocument();
  });

  it('renders nothing when user lacks the permission and no fallback provided', () => {
    mockHasPermission.mockReturnValue(false);

    const { container } = render(
      <PermissionGate permission="roles.manage">
        <span>Protected content</span>
      </PermissionGate>,
    );

    expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
    // Fragment with null child renders empty
    expect(container.firstChild).toBeNull();
  });

  it('renders fallback when user lacks permission and fallback is provided', () => {
    mockHasPermission.mockReturnValue(false);

    render(
      <PermissionGate
        permission="roles.manage"
        fallback={<span>Access denied</span>}
      >
        <span>Protected content</span>
      </PermissionGate>,
    );

    expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
    expect(screen.getByText('Access denied')).toBeInTheDocument();
  });
});
