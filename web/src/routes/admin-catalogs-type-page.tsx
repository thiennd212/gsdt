import { createRoute } from '@tanstack/react-router';
import { adminRoute } from './admin-layout';
import { GenericCatalogListPage } from '@/features/admin-catalogs';

// Route: /admin/catalogs/$type — generic catalog CRUD page (10 catalog types)
export const adminCatalogTypeRoute = createRoute({
  getParentRoute: () => adminRoute,
  path: '/admin/catalogs/$type',
  component: function CatalogTypeRouteComponent() {
    const { type } = adminCatalogTypeRoute.useParams();
    return <GenericCatalogListPage catalogType={type} />;
  },
});
