import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /domestic-projects/new — create new domestic project
export const domesticProjectCreateRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/domestic-projects/new',
  component: lazyRouteComponent(() => import('@/features/domestic-projects'), 'DomesticProjectCreatePage'),
});
