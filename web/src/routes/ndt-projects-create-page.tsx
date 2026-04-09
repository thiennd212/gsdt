import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /ndt-projects/new — create new NĐT project
export const ndtProjectCreateRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ndt-projects/new',
  component: lazyRouteComponent(() => import('@/features/ndt-projects'), 'NdtProjectCreatePage'),
});
