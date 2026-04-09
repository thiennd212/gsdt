import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /fdi-projects — FDI project list
export const fdiProjectsRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/fdi-projects',
  component: lazyRouteComponent(() => import('@/features/fdi-projects'), 'FdiProjectListPage'),
});
