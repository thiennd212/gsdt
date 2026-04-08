import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { OdaProjectEditPage } from '@/features/oda-projects';

export const odaProjectEditRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/oda-projects/$id/edit',
  component: function OdaEditRouteComponent() {
    const { id } = odaProjectEditRoute.useParams();
    return <OdaProjectEditPage projectId={id} />;
  },
});
