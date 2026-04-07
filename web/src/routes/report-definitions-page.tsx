import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /reports/definitions — report template list + create
export const reportDefinitionsRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/reports/definitions',
  component: lazyRouteComponent(() => import('@/features/reports'), 'ReportDefinitionsPage'),
});
