import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /domestic-projects — domestic project list
export const domesticProjectsRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/domestic-projects',
  component: lazyRouteComponent(() => import('@/features/domestic-projects'), 'DomesticProjectListPage'),
});
