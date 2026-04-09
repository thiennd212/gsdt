import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /ndt-projects — NĐT project list
export const ndtProjectsRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ndt-projects',
  component: lazyRouteComponent(() => import('@/features/ndt-projects'), 'NdtProjectListPage'),
});
