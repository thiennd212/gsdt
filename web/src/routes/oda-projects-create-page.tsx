import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

export const odaProjectCreateRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/oda-projects/new',
  component: lazyRouteComponent(() => import('@/features/oda-projects'), 'OdaProjectCreatePage'),
});
