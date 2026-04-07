import { createRoute, Outlet } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { usePermissions } from '@/features/auth';
import { ForbiddenPage } from '@/shared/components/forbidden-page';

// Admin layout route — enforces Admin/SystemAdmin role for all /admin/* routes.
// Prevents direct URL access without proper role (sidebar already filters menu items).
export const adminRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  id: '_admin',
  component: AdminGuard,
});

function AdminGuard() {
  const { roles } = usePermissions();
  const isAdmin = roles.includes('Admin') || roles.includes('SystemAdmin');

  if (!isAdmin) {
    return <ForbiddenPage />;
  }

  return <Outlet />;
}
