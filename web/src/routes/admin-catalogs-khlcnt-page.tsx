import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { adminRoute } from './admin-layout';

// Route: /admin/catalogs/contractor-selection-plans — KHLCNT dedicated page
export const adminCatalogsKhlcntRoute = createRoute({
  getParentRoute: () => adminRoute,
  path: '/admin/catalogs/contractor-selection-plans',
  component: lazyRouteComponent(() => import('@/features/admin-catalogs'), 'KhlcntCatalogPage'),
});
