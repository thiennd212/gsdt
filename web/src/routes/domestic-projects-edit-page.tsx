import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { DomesticProjectEditPage } from '@/features/domestic-projects';

// Route: /domestic-projects/$id/edit — edit domestic project
export const domesticProjectEditRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/domestic-projects/$id/edit',
  component: function DomesticEditRouteComponent() {
    const { id } = domesticProjectEditRoute.useParams();
    return <DomesticProjectEditPage projectId={id} />;
  },
});
