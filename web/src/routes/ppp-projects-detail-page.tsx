import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { PppProjectDetailPage } from '@/features/ppp-projects';

// Route: /ppp-projects/$id — view PPP project detail (readonly)
export const pppProjectDetailRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ppp-projects/$id',
  component: function PppDetailRouteComponent() {
    const { id } = pppProjectDetailRoute.useParams();
    return <PppProjectDetailPage projectId={id} />;
  },
});
