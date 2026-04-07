import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /chat — real-time conversation with SignalR (P2-01)
export const chatRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/chat',
  component: lazyRouteComponent(
    () => import('@/features/collaboration'),
    'ChatPage',
  ),
});
