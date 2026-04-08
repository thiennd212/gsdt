import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /dnnn-projects — DNNN project list
export const dnnnProjectsRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/dnnn-projects',
  component: lazyRouteComponent(() => import('@/features/dnnn-projects'), 'DnnnProjectListPage'),
});
