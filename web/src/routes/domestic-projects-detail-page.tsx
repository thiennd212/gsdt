import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { DomesticProjectDetailPage } from '@/features/domestic-projects';

// Route: /domestic-projects/$id — view domestic project detail (readonly)
export const domesticProjectDetailRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/domestic-projects/$id',
  component: function DomesticDetailRouteComponent() {
    const { id } = domesticProjectDetailRoute.useParams();
    return <DomesticProjectDetailPage projectId={id} />;
  },
});
