import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /signatures — signature request list and detail (P2-02)
export const signaturesRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/signatures',
  component: lazyRouteComponent(
    () => import('@/features/signature'),
    'SignatureListPage',
  ),
  validateSearch: (search: Record<string, unknown>) => ({
    page: Number(search['page'] ?? 1),
    pageSize: Number(search['pageSize'] ?? 20),
  }),
});
