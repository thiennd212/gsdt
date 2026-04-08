import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /dnnn-projects/new — create new DNNN project
export const dnnnProjectCreateRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/dnnn-projects/new',
  component: lazyRouteComponent(() => import('@/features/dnnn-projects'), 'DnnnProjectCreatePage'),
});
