import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /ai/search — AI query catalog and NLQ execution
export const aiSearchRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ai/search',
  component: lazyRouteComponent(() => import('@/features/ai'), 'AiSearchPage'),
});
