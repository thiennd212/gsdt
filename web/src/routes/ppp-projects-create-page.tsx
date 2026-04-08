import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /ppp-projects/new — create new PPP project
export const pppProjectCreateRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ppp-projects/new',
  component: lazyRouteComponent(() => import('@/features/ppp-projects'), 'PppProjectCreatePage'),
});
