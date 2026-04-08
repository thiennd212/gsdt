import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

export const odaProjectsRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/oda-projects',
  component: lazyRouteComponent(() => import('@/features/oda-projects'), 'OdaProjectListPage'),
});
