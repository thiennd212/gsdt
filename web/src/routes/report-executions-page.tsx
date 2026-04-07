import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /reports/executions — execution history + polling + download
export const reportExecutionsRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/reports/executions',
  component: lazyRouteComponent(() => import('@/features/reports'), 'ReportExecutionsPage'),
});
