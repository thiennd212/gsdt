import { createRoute } from '@tanstack/react-router';
import { authenticatedRoute } from './authenticated-layout';
import { NdtProjectDetailPage } from '@/features/ndt-projects';

// Route: /ndt-projects/$id — view NĐT project detail (readonly)
export const ndtProjectDetailRoute = createRoute({
  getParentRoute: () => authenticatedRoute,
  path: '/ndt-projects/$id',
  component: function NdtDetailRouteComponent() {
    const { id } = ndtProjectDetailRoute.useParams();
    return <NdtProjectDetailPage projectId={id} />;
  },
});
