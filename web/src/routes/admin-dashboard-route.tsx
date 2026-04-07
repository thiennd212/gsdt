import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { adminRoute } from './admin-layout';

// Route: exact /admin path under admin guard — renders AdminDashboardPage
export const adminDashboardRoute = createRoute({
  getParentRoute: () => adminRoute,
  path: '/admin',
  component: lazyRouteComponent(
    () => import('@/features/admin-dashboard'),
    'AdminDashboardPage',
  ),
});
