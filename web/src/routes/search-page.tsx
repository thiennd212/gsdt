import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /search — unified multi-entity search with facets (P1-08)
export const searchRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/search',
  component: lazyRouteComponent(
    () => import('@/features/search'),
    'UnifiedSearchPage',
  ),
});
