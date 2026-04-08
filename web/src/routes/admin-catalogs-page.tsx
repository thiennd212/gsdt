import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { adminRoute } from './admin-layout';

// Route: /admin/catalogs — catalog navigation hub (all 11 catalogs)
export const adminCatalogsRoute = createRoute({
  getParentRoute: () => adminRoute,
  path: '/admin/catalogs',
  component: lazyRouteComponent(() => import('@/features/admin-catalogs'), 'CatalogIndexPage'),
});
