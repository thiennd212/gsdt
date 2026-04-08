import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { OdaProjectDetailPage } from '@/features/oda-projects';

export const odaProjectDetailRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/oda-projects/$id',
  component: function OdaDetailRouteComponent() {
    const { id } = odaProjectDetailRoute.useParams();
    return <OdaProjectDetailPage projectId={id} />;
  },
});
