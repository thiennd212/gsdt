import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';

// TC-FE-LAY-002: SidebarMenu renders menu items for admin role
// TC-FE-LAY-003: SidebarMenu hides admin items for regular user

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => vi.fn(),
  useLocation: () => ({ pathname: '/' }),
}));

import { SidebarMenu } from '@/layouts/sidebar-menu';

describe('SidebarMenu — TC-FE-LAY-002: admin role sees admin group', () => {
  it('renders admin sub-menu for SystemAdmin role', () => {
    render(<SidebarMenu permissions={[]} roles={['SystemAdmin']} />);
    // The admin group label uses i18n key nav.admin
    expect(screen.getByText('nav.admin')).toBeTruthy();
  });

  it('renders standard nav items for all users', () => {
    render(<SidebarMenu permissions={[]} roles={[]} />);
    expect(screen.getByText('nav.dashboard')).toBeTruthy();
    expect(screen.getByText('nav.cases')).toBeTruthy();
    expect(screen.getByText('nav.inbox')).toBeTruthy();
  });

  it('renders audit item when roles include GovOfficer', () => {
    render(<SidebarMenu permissions={[]} roles={['GovOfficer']} />);
    expect(screen.getByText('nav.audit')).toBeTruthy();
  });
});

describe('SidebarMenu — TC-FE-LAY-003: regular user has no admin group', () => {
  it('does not render admin sub-menu for user with no roles/permissions', () => {
    render(<SidebarMenu permissions={[]} roles={[]} />);
    // nav.admin is the sub-menu label; should not appear for plain users
    expect(screen.queryByText('nav.admin')).toBeNull();
  });

  it('does not render audit item for regular user', () => {
    render(<SidebarMenu permissions={[]} roles={[]} />);
    expect(screen.queryByText('nav.audit')).toBeNull();
  });

  it('does not render roles item for regular user', () => {
    render(<SidebarMenu permissions={[]} roles={[]} />);
    expect(screen.queryByText('nav.roles')).toBeNull();
  });
});
