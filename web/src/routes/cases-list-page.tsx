import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /cases — case list, requires auth
export const casesRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/cases',
  component: lazyRouteComponent(() => import('@/features/cases'), 'CaseListPage'),
  validateSearch: (search: Record<string, unknown>) => ({
    page: Number(search['page'] ?? 1),
    pageSize: Number(search['pageSize'] ?? 20),
  }),
});
