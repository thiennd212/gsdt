import { createRoute, lazyRouteComponent } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';

// Route: /copilot — standalone AI copilot chat with SSE streaming (P1-13)
export const copilotRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/copilot',
  component: lazyRouteComponent(
    () => import('@/features/copilot'),
    'CopilotChatPage',
  ),
});
