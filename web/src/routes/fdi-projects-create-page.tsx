import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /fdi-projects/new — create new FDI project
export const fdiProjectCreateRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/fdi-projects/new',
  component: lazyRouteComponent(() => import('@/features/fdi-projects'), 'FdiProjectCreatePage'),
});
