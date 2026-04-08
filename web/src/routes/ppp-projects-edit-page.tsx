import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { PppProjectEditPage } from '@/features/ppp-projects';

// Route: /ppp-projects/$id/edit — edit PPP project
export const pppProjectEditRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ppp-projects/$id/edit',
  component: function PppEditRouteComponent() {
    const { id } = pppProjectEditRoute.useParams();
    return <PppProjectEditPage projectId={id} />;
  },
});
