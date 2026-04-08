import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /ppp-projects — PPP project list
export const pppProjectsRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ppp-projects',
  component: lazyRouteComponent(() => import('@/features/ppp-projects'), 'PppProjectListPage'),
});
